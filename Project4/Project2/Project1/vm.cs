using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm
{
    unsafe class vm
    {
        static Dictionary<string, int> opCodes = new Dictionary<string, int>();
        static Dictionary<int, string> reverseOpCodes = new Dictionary<int, string>();
        static Dictionary<string, int> directives = new Dictionary<string, int>();

        static int PC;
        static int printVariable;
        static Dictionary<string, int> symbolTable = new Dictionary<string, int>();
        static Dictionary<int, string> reverseSymbolTable = new Dictionary<int, string>();
        static Dictionary<string, int> specialRegisters = new Dictionary<string, int>();
        static bool[] Threads = new bool[5];
        const int MEM_SIZE = 20000;
        const int ThreadStackSize = 1000;

        unsafe static void Main(string[] args)
        {
            try
            {
                #region Fill OpCodes
                opCodes.Add("JMP", 1);
                opCodes.Add("JMR", 2);
                opCodes.Add("BNZ", 3);
                opCodes.Add("BGT", 4);
                opCodes.Add("BLT", 5);
                opCodes.Add("BRZ", 6);
                opCodes.Add("MOV", 7);
                opCodes.Add("LDA", 8);
                opCodes.Add("STR", 9);
                opCodes.Add("LDR", 10);
                opCodes.Add("STB", 11);
                opCodes.Add("LDB", 12);
                opCodes.Add("ADD", 13);
                opCodes.Add("ADI", 14);
                opCodes.Add("SUB", 15);
                opCodes.Add("MUL", 16);
                opCodes.Add("DIV", 17);
                opCodes.Add("AND", 18);
                opCodes.Add("OR", 19);
                opCodes.Add("CMP", 20);
                opCodes.Add("TRP", 21);
                opCodes.Add("RUN", 26);
                opCodes.Add("END", 27);
                opCodes.Add("BLK", 28);
                opCodes.Add("LCK", 29);
                opCodes.Add("ULK", 30);

                foreach (var item in opCodes)
                {
                    reverseOpCodes.Add(item.Value, item.Key);
                }
                reverseOpCodes.Add(22, "STR");
                reverseOpCodes.Add(23, "LDR");
                reverseOpCodes.Add(24, "STB");
                reverseOpCodes.Add(25, "LDB");
                #endregion

                #region Fill Directives
                directives.Add(".INT", 4);
                directives.Add(".BYT", 1);
                #endregion

                #region Load Threads
                Threads[0] = true;
                Threads[1] = false;
                Threads[2] = false;
                Threads[3] = false;
                Threads[4] = false;
                #endregion

                #region Fill Special Registers
                specialRegisters.Add("PC", 8);
                specialRegisters.Add("SP", 9);
                specialRegisters.Add("FP", 10);
                specialRegisters.Add("SL", 11);
                specialRegisters.Add("SB", 12);
                #endregion

                bool debug = false;
                if (args.Length > 1)
                {
                    debug = true;
                }
                string label = "";
                string opCode = "";
                string op1 = "";
                string op2 = "";
                string input = "";
                int counter = 0;
                int locationCounter = 0;
                int currentThreadID = 0;
                bool contextSwitch = false;
                bool startProgram = false;
                int[] Registers = new int[8 + specialRegisters.Count];
                #region Load Symbol Table
                StreamReader sr = File.OpenText(args[0]);
                while ((input = sr.ReadLine()) != null)
                {
                    counter = 0;
                    opCode = null;
                    label = null;
                    op1 = null;
                    op2 = null;
                    var words = input.Trim().Split().Select(x => x.Trim(' '));
                    if (words.Count() == 2)
                    {
                        foreach (var word in words)
                        {
                            if (word == string.Empty || (word[0] == '\\' && word[1] == '\\'))
                            {
                                break;
                            }
                            else if (counter == 0)
                            {
                                if (opCodes.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else if (directives.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else
                                {
                                    label = word;
                                }
                            }
                            else
                            {
                                op1 = word;
                                op2 = null;
                                counter = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var word in words)
                        {
                            if (word == string.Empty || (word[0] == '\\' && word[1] == '\\') || (word[0] == '/' && word[1] == '/'))
                            {
                                break;
                            }
                            else if (counter == 0)
                            {
                                if (opCodes.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else if (directives.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else
                                {
                                    label = word;
                                }
                            }
                            else if (counter == 1)
                            {
                                //if(opCodes.ContainsKey(opCode) && !int.TryParse(word, out int value))
                                //{
                                //    label = word;
                                //}
                                op1 = word;
                                op2 = null;
                                counter++;
                            }
                            else
                            {
                                op2 = word;
                                counter = 0;
                            }
                        }
                    }
                    if (opCode != null && opCodes.ContainsKey(opCode))
                    {
                        if (!startProgram)
                        {
                            PC = locationCounter;
                            startProgram = true;
                        }
                        if (label != null && !symbolTable.ContainsKey(label))
                        {
                            symbolTable.Add(label, locationCounter);
                        }
                        //locationCounter += 12;
                        locationCounter += 12;
                    }
                    else if (opCode != null && directives.ContainsKey(opCode))
                    {
                        if (label != null && !symbolTable.ContainsKey(label))
                        {
                            symbolTable.Add(label, locationCounter);
                        }
                        //locationCounter += directives[opCode];
                        if (opCode == ".BYT")
                        {
                            locationCounter += 1;
                        }
                        else
                        {
                            locationCounter += 4;
                        }
                    }
                }
                Registers[specialRegisters["SL"]] = locationCounter;
                Registers[specialRegisters["SB"]] = MEM_SIZE;
                Registers[specialRegisters["SP"]] = MEM_SIZE;
                foreach (var item in symbolTable)
                {
                    reverseSymbolTable.Add(item.Value, item.Key);
                }
                sr.Close();

                if (debug)
                {
                    Console.WriteLine("Symbol Table:");
                    foreach (var value in symbolTable)
                    {
                        Console.WriteLine("{0}->{1}", value.Key, value.Value);
                    }
                }
                #endregion

                #region Load Byte Code
                printVariable = PC;
                byte* mem = stackalloc byte[MEM_SIZE];
                byte* newP2 = &mem[PC];
                int currValue = 0;
                sr = File.OpenText(args[0]);
                while ((input = sr.ReadLine()) != null)
                {
                    counter = 0;
                    opCode = null;
                    label = null;
                    op1 = null;
                    op2 = null;
                    var words = input.Trim().Split().Select(x => x.Trim(' '));
                    if (words.Count() == 2)
                    {
                        foreach (var word in words)
                        {
                            if (word == string.Empty || (word[0] == '\\' && word[1] == '\\') || (word[0] == '/' && word[1] == '/'))
                            {
                                break;
                            }
                            else if (counter == 0)
                            {
                                if (opCodes.ContainsKey(word))
                                {
                                    label = word;
                                    opCode = word;
                                    counter++;
                                }
                                else if (directives.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                            }
                            else
                            {
                                op1 = word;
                                op2 = null;
                                counter = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var word in words)
                        {
                            if (word == string.Empty || (word[0] == '\\' && word[1] == '\\') || (word[0] == '/' && word[1] == '/'))
                            {
                                break;
                            }
                            else if (counter == 0)
                            {
                                if (opCodes.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else if (directives.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else
                                {
                                    label = word;
                                }
                            }
                            else if (counter == 1)
                            {
                                if (opCodes.ContainsKey(opCode) && !int.TryParse(word, out int value))
                                {
                                    label = word;
                                }
                                op1 = word;
                                op2 = null;
                                counter++;
                            }
                            else
                            {
                                op2 = word;
                                counter = 0;
                            }
                        }
                    }
                    if (opCode != null && opCodes.ContainsKey(opCode))
                    {
                        if ((opCode == "LDR" || opCode == "STR" || opCode == "STB" || opCode == "LDB") && ((op2.Length == 2 && op2[0] == 'R') || specialRegisters.ContainsKey(op2)))
                        {
                            if (opCode == "STR")
                            {
                                int value = 22;
                                byte[] array = BitConverter.GetBytes(value);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                            else if (opCode == "LDR")
                            {
                                int value = 23;
                                byte[] array = BitConverter.GetBytes(value);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                            else if (opCode == "STB")
                            {
                                int value = 24;
                                byte[] array = BitConverter.GetBytes(value);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                            else if (opCode == "LDB")
                            {
                                int value = 25;
                                byte[] array = BitConverter.GetBytes(value);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                        }
                        else
                        {
                            int value = opCodes[opCode];
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                        }
                        //newP2 += 4;
                        //newP2++;
                        if (op1 == null)
                        {
                            newP2 += 4;
                            //newP2++;
                        }
                        else if (op1.Length == 2 && op1[0] == 'R')
                        {
                            int value = int.Parse(op1[1].ToString());
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                        else if (specialRegisters.ContainsKey(op1))
                        {
                            byte[] array = BitConverter.GetBytes(specialRegisters[op1]);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                        }
                        else if (symbolTable.ContainsKey(op1))
                        {
                            int value = symbolTable[op1];
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                        else
                        {
                            int value = int.Parse(op1);
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                        if (op2 == null)
                        {
                            newP2 += 4;
                            //newP2++;
                        }
                        else if (op2.Length == 2 && op2[0] == 'R')
                        {
                            int value = int.Parse(op2[1].ToString());
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                        else if (specialRegisters.ContainsKey(op2))
                        {
                            byte[] array = BitConverter.GetBytes(specialRegisters[op2]);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                        }
                        else if (symbolTable.ContainsKey(op2))
                        {
                            int value = symbolTable[op2];
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                        else
                        {
                            int value = int.Parse(op2);
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP2 = bit;
                                newP2++;
                            }
                            //newP2++;
                        }
                    }
                    else if (opCode != null && directives.ContainsKey(opCode))
                    {
                        byte* newP;
                        if (label != null)
                        {
                            if (!symbolTable.ContainsKey(label))
                            {
                                Console.WriteLine("Label Not Added To Symbol Table.");
                                break;
                            }
                            currValue = symbolTable[label];
                        }
                        else
                        {
                            if (opCode == ".BYT")
                            {
                                currValue += 1;
                            }
                            else
                            {
                                currValue += 4;
                            }
                        }
                        newP = &mem[currValue];
                        if (opCode == ".INT")
                        {
                            int value = Convert.ToInt32(op1);
                            byte[] array = BitConverter.GetBytes(value);
                            foreach (var bit in array)
                            {
                                *newP = bit;
                                newP++;
                            }
                        }
                        else
                        {
                            char[] c = op1.ToCharArray();
                            if (c.Length > 1)
                            {
                                var array = BitConverter.GetBytes(c[1]);
                                *newP = array[0];
                            }
                            else
                            {
                                var array = BitConverter.GetBytes(c[0]);
                                *newP = array[0];
                            }
                        }
                    }
                }
                sr.Close();
                if (debug)
                {
                    byte* p = mem;
                    Console.WriteLine("\n\nByte Table:");
                    for (int i = 0; i < printVariable; i++)
                    {
                        Console.WriteLine("{0}: {1}", i, *p);
                        p++;
                    }
                    Console.WriteLine();
                }
                #endregion

                #region Run Assembly Code
                byte* currP;
                int intOp1 = 0;
                int intOp2 = 0;
                Registers[specialRegisters["PC"]] = PC;
                int newThreadID = -1;
                int newThreadPC = -1;
                int countyer = 0;
                bool running = true;
                byte[] opArray = new byte[4];
                byte[] op1Array = new byte[4];
                byte[] op2Array = new byte[4];
                while (running)
                {
                    PC = Registers[specialRegisters["PC"]];
                    currP = &mem[PC];
                    for (int i = 0; i < 4; i++)
                    {
                        opArray[i] = *currP;
                        currP++;
                    }
                    int opCodeInt = BitConverter.ToInt32(opArray, 0);
                    if (reverseOpCodes.ContainsKey(opCodeInt))
                    {
                        opCode = reverseOpCodes[opCodeInt];
                        for (int i = 0; i < 4; i++)
                        {
                            op1Array[i] = *currP;
                            currP++;
                        }
                        intOp1 = BitConverter.ToInt32(op1Array, 0);
                        for (int i = 0; i < 4; i++)
                        {
                            op2Array[i] = *currP;
                            currP++;
                        }
                        intOp2 = BitConverter.ToInt32(op2Array, 0);
                        switch (opCode)
                        {
                            case "JMP":
                                if (reverseSymbolTable.ContainsKey(intOp1))
                                {
                                    PC = intOp1 - 12;
                                }
                                else
                                {
                                    Console.WriteLine("There is no label in location: " + intOp1 + ".");
                                    running = false;
                                }
                                break;
                            case "JMR":
                                break;
                            case "BNZ":
                                if (Registers[intOp1] != 0)
                                {
                                    if (reverseSymbolTable.ContainsKey(intOp2))
                                    {
                                        PC = intOp2 - 12;
                                    }
                                    else
                                    {
                                        Console.WriteLine("There is no label in location: " + intOp2 + ".");
                                        running = false;
                                    }
                                }
                                break;
                            case "BGT":
                                if (Registers[intOp1] > 0)
                                {
                                    if (reverseSymbolTable.ContainsKey(intOp2))
                                    {
                                        PC = intOp2 - 12;
                                    }
                                    else
                                    {
                                        Console.WriteLine("There is no label in location: " + intOp2 + ".");
                                        running = false;
                                    }
                                }
                                break;
                            case "BLT":
                                if (Registers[intOp1] < 0)
                                {
                                    if (reverseSymbolTable.ContainsKey(intOp2))
                                    {
                                        PC = intOp2 - 12;
                                    }
                                    else
                                    {
                                        Console.WriteLine("There is no label in location: " + intOp2 + ".");
                                        running = false;
                                    }
                                }
                                break;
                            case "BRZ":
                                if (Registers[intOp1] == 0)
                                {
                                    if (reverseSymbolTable.ContainsKey(intOp2))
                                    {
                                        PC = intOp2 - 12;
                                    }
                                    else
                                    {
                                        Console.WriteLine("There is no label in location: " + intOp2 + ".");
                                        running = false;
                                    }
                                }
                                break;
                            case "MOV":
                                Registers[intOp1] = Registers[intOp2];
                                break;
                            case "LDA":
                                if (reverseSymbolTable.ContainsKey(intOp2))
                                {
                                    Registers[intOp1] = intOp2;
                                }
                                else
                                {
                                    Console.WriteLine("There is no label in location: " + intOp2 + ".");
                                    running = false;
                                }
                                break;
                            case "STR":
                                if (opCodeInt == 22)
                                {
                                    int length = 4;
                                    byte[] bty = BitConverter.GetBytes(Registers[intOp1]);
                                    if (bty.Length == 2)
                                    {
                                        length = 1;
                                    }
                                    for (int i = 0; i < length; i++)
                                    {
                                        mem[Registers[intOp2] + i] = bty[i];
                                    }
                                    bty = null;
                                }
                                else
                                {
                                    int length = 4;
                                    byte[] bty = BitConverter.GetBytes(Registers[intOp1]);
                                    if (bty.Length == 2)
                                    {
                                        length = 1;
                                    }
                                    for (int i = 0; i < length; i++)
                                    {
                                        mem[intOp2 + i] = bty[i];
                                    }
                                    bty = null;
                                }
                                break;
                            case "LDR":
                                if (opCodeInt == 23)
                                {
                                    byte[] bty = { mem[Registers[intOp2]], mem[Registers[intOp2] + 1],
                                    mem[Registers[intOp2] + 2], mem[Registers[intOp2] + 3] };
                                    Registers[intOp1] = BitConverter.ToInt32(bty, 0);
                                    if (intOp1 == 8)
                                    {
                                        PC = Registers[intOp1];
                                    }
                                    bty = null;
                                }
                                else
                                {
                                    byte[] bty = { mem[intOp2], mem[intOp2 + 1], mem[intOp2 + 2], mem[intOp2 + 3] };
                                    Registers[intOp1] = BitConverter.ToInt32(bty, 0);
                                    bty = null;
                                }
                                break;
                            case "STB":
                                if (opCodeInt == 24)
                                {
                                    byte[] stb = BitConverter.GetBytes(Registers[intOp1]);
                                    mem[Registers[intOp2]] = stb[0];
                                    stb = null;
                                }
                                else
                                {
                                    byte[] stb = BitConverter.GetBytes(Registers[intOp1]);
                                    mem[intOp2] = stb[0];
                                    stb = null;
                                }
                                break;
                            case "LDB":
                                if (intOp1 == 3 && intOp2 == 161)
                                { }
                                if (opCodeInt == 25)
                                {
                                    byte[] ldb = BitConverter.GetBytes(mem[Registers[intOp2]]);
                                    Registers[intOp1] = ldb[0];
                                    ldb = null;
                                }
                                else
                                {
                                    byte[] ldb = BitConverter.GetBytes(mem[intOp2]);
                                    Registers[intOp1] = ldb[0];
                                    ldb = null;
                                }
                                break;
                            case "ADD":
                                Registers[intOp1] += Registers[intOp2];
                                break;
                            case "ADI":
                                Registers[intOp1] += intOp2;
                                break;
                            case "SUB":
                                Registers[intOp1] -= Registers[intOp2];
                                break;
                            case "MUL":
                                Registers[intOp1] *= Registers[intOp2];
                                break;
                            case "DIV":
                                Registers[intOp1] /= Registers[intOp2];
                                break;
                            case "AND":
                                break;
                            case "OR":
                                break;
                            case "RUN":
                                bool found = false;
                                for (int i = 1; i < Threads.Length; i++)
                                {
                                    if (!Threads[i])
                                    {
                                        Registers[intOp1] = i;
                                        Threads[i] = true;
                                        found = true;
                                        contextSwitch = true;
                                        newThreadID = i;
                                        newThreadPC = intOp2;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    Console.WriteLine("Too Many Thread Instances Initiated!");
                                    running = false;
                                }
                                break;
                            case "END":
                                if (currentThreadID != 0)
                                {
                                    Threads[currentThreadID] = false;
                                }
                                break;
                            case "BLK":
                                if (currentThreadID == 0)
                                {
                                    bool block = false;
                                    foreach (bool thread in Threads.Skip(1))
                                    {
                                        if (thread)
                                        {
                                            block = true;
                                        }
                                    }
                                    if (block)
                                    {
                                        PC -= 12;
                                    }
                                }
                                break;
                            case "LCK":
                                byte[] mbty = { mem[intOp1], mem[intOp1 + 1], mem[intOp1 + 2], mem[intOp1 + 3] };
                                int mutex = BitConverter.ToInt32(mbty, 0);
                                if (mutex != -1)
                                {
                                    PC -= 12;
                                }
                                else
                                {
                                    byte[] bty = BitConverter.GetBytes(currentThreadID);
                                    for (int i = 0; i < bty.Length; i++)
                                    {
                                        mem[intOp1 + i] = bty[i];
                                    }
                                    bty = null;
                                }
                                mbty = null;
                                break;
                            case "ULK":
                                byte[] umbty = { mem[intOp1], mem[intOp1 + 1], mem[intOp1 + 2], mem[intOp1 + 3] };
                                int umutex = BitConverter.ToInt32(umbty, 0);
                                if (umutex == currentThreadID)
                                {
                                    byte[] bty = BitConverter.GetBytes(-1);
                                    for (int i = 0; i < 4; i++)
                                    {
                                        mem[intOp1 + i] = bty[i];
                                    }
                                    bty = null;
                                }
                                umbty = null;
                                break;
                            case "CMP":
                                if (Registers[intOp1] == Registers[intOp2])
                                {
                                    Registers[intOp1] = 0;
                                }
                                else if (Registers[intOp1] > Registers[intOp2])
                                {
                                    Registers[intOp1] = 1;
                                }
                                else if (Registers[intOp1] < Registers[intOp2])
                                {
                                    Registers[intOp1] = -1;
                                }
                                break;
                            case "TRP":
                                switch (intOp1)
                                {
                                    case 0:
                                        running = false;
                                        break;
                                    case 1:
                                        Console.Write("{0}", (int)Registers[3]);
                                        break;
                                    case 2:
                                        Registers[3] = int.Parse(Console.ReadLine());
                                        break;
                                    case 3:
                                        char c = (char)Registers[3];
                                        if (c == '~')
                                        {
                                            Console.WriteLine();
                                        }
                                        else if (c == '`')
                                        {
                                            Console.Write(" ");
                                        }
                                        else
                                        {
                                            Console.Write("{0}", c);
                                        }
                                        break;
                                    case 4:
                                        var value = Console.ReadKey();
                                        Registers[3] = value.KeyChar;
                                        break;
                                    case 100:
                                        if (Registers[0] == 144)
                                        {
                                            countyer = 0;
                                        }
                                        else
                                        {
                                            countyer++;
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                    PC += 12;
                    Registers[specialRegisters["PC"]] = PC;
                    if (Registers[specialRegisters["SP"]] < Registers[specialRegisters["SL"]])
                    {
                        Console.WriteLine("Stack Overflow Occured!!!");
                        break;
                    }
                    if (Registers[specialRegisters["SP"]] > Registers[specialRegisters["SB"]])
                    {
                        Console.WriteLine("Stack Underflow Occured!!!");
                        break;
                    }
                    if (contextSwitch)
                    {
                        int oldThreadID = currentThreadID;
                        if (currentThreadID == 0)
                        {
                            if (Threads[1])
                            {
                                currentThreadID = 1;
                            }
                            else if (Threads[2])
                            {
                                currentThreadID = 2;
                            }
                            else if (Threads[3])
                            {
                                currentThreadID = 3;
                            }
                            else if (Threads[4])
                            {
                                currentThreadID = 4;
                            }
                        }
                        else if (currentThreadID == 1)
                        {
                            if (Threads[2])
                            {
                                currentThreadID = 2;
                            }
                            else if (Threads[3])
                            {
                                currentThreadID = 3;
                            }
                            else if (Threads[4])
                            {
                                currentThreadID = 4;
                            }
                            else if (Threads[0])
                            {
                                currentThreadID = 0;
                            }
                        }
                        else if (currentThreadID == 2)
                        {
                            if (Threads[3])
                            {
                                currentThreadID = 3;
                            }
                            else if (Threads[4])
                            {
                                currentThreadID = 4;
                            }
                            else if (Threads[0])
                            {
                                currentThreadID = 0;
                            }
                            else if (Threads[1])
                            {
                                currentThreadID = 1;
                            }
                        }
                        else if (currentThreadID == 3)
                        {
                            if (Threads[4])
                            {
                                currentThreadID = 4;
                            }
                            else if (Threads[0])
                            {
                                currentThreadID = 0;
                            }
                            else if (Threads[1])
                            {
                                currentThreadID = 1;
                            }
                            else if (Threads[2])
                            {
                                currentThreadID = 2;
                            }
                        }
                        else if (currentThreadID == 4)
                        {
                            if (Threads[0])
                            {
                                currentThreadID = 0;
                            }
                            else if (Threads[1])
                            {
                                currentThreadID = 1;
                            }
                            else if (Threads[2])
                            {
                                currentThreadID = 2;
                            }
                            else if (Threads[3])
                            {
                                currentThreadID = 3;
                            }
                        }
                        if (currentThreadID != oldThreadID)
                        {
                            int threadSB = MEM_SIZE - (oldThreadID * ThreadStackSize);
                            for (int i = 0; i < Registers.Length; i++)
                            {
                                int memLocation = 0;
                                if (i == specialRegisters["PC"])
                                {
                                    memLocation = threadSB - 4;
                                }
                                else if (i == 0)
                                {
                                    memLocation = threadSB - 8;
                                }
                                else if (i == 1)
                                {
                                    memLocation = threadSB - 12;
                                }
                                else if (i == 2)
                                {
                                    memLocation = threadSB - 16;
                                }
                                else if (i == 3)
                                {
                                    memLocation = threadSB - 20;
                                }
                                else if (i == 4)
                                {
                                    memLocation = threadSB - 24;
                                }
                                else if (i == 5)
                                {
                                    memLocation = threadSB - 28;
                                }
                                else if (i == 6)
                                {
                                    memLocation = threadSB - 32;
                                }
                                else if (i == 7)
                                {
                                    memLocation = threadSB - 36;
                                }
                                else if (i == specialRegisters["SP"])
                                {
                                    memLocation = threadSB - 40;
                                }
                                else if (i == specialRegisters["FP"])
                                {
                                    memLocation = threadSB - 44;
                                }
                                int length = 4;
                                byte[] bty = BitConverter.GetBytes(Registers[i]);
                                if (bty.Length == 2)
                                {
                                    length = 1;
                                }
                                for (int j = 0; j < length; j++)
                                {
                                    mem[memLocation + j] = bty[j];
                                }
                                bty = null;
                            }
                            threadSB = MEM_SIZE - (currentThreadID * ThreadStackSize);
                            if (currentThreadID == newThreadID)
                            {
                                newThreadID = -1;
                                Registers[specialRegisters["PC"]] = newThreadPC;
                                PC = newThreadPC;
                                newThreadPC = -1;
                                Registers[specialRegisters["SP"]] = threadSB;
                                Registers[specialRegisters["FP"]] = threadSB;
                                Registers[0] = 0;
                                Registers[1] = 0;
                                Registers[2] = 0;
                                Registers[3] = 0;
                                Registers[4] = 0;
                                Registers[5] = 0;
                                Registers[6] = 0;
                                Registers[7] = 0;
                            }
                            else
                            {
                                threadSB -= 4; // PC
                                byte[] pcbty = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[specialRegisters["PC"]] = BitConverter.ToInt32(pcbty, 0);
                                PC = Registers[specialRegisters["PC"]];
                                threadSB -= 4; // 0
                                pcbty = null;
                                byte[] bty0 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[0] = BitConverter.ToInt32(bty0, 0);
                                bty0 = null;
                                threadSB -= 4; // 1
                                byte[] bty1 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[1] = BitConverter.ToInt32(bty1, 0);
                                bty1 = null;
                                threadSB -= 4; // 2
                                byte[] bty2 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[2] = BitConverter.ToInt32(bty2, 0);
                                bty2 = null;
                                threadSB -= 4; // 3
                                byte[] bty3 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[3] = BitConverter.ToInt32(bty3, 0);
                                bty3 = null;
                                threadSB -= 4; // 4
                                byte[] bty4 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[4] = BitConverter.ToInt32(bty4, 0);
                                bty4 = null;
                                threadSB -= 4; // 5
                                byte[] bty5 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[5] = BitConverter.ToInt32(bty5, 0);
                                bty5 = null;
                                threadSB -= 4; // 6
                                byte[] bty6 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[6] = BitConverter.ToInt32(bty6, 0);
                                bty6 = null;
                                threadSB -= 4; // 7
                                byte[] bty7 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[7] = BitConverter.ToInt32(bty7, 0);
                                bty7 = null;
                                threadSB -= 4; // SP
                                byte[] btysp = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[specialRegisters["SP"]] = BitConverter.ToInt32(btysp, 0);
                                btysp = null;
                                threadSB -= 4; // FP
                                byte[] btyfp = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                Registers[specialRegisters["FP"]] = BitConverter.ToInt32(btyfp, 0);
                                btyfp = null;
                            }
                        }
                        bool cont = false;
                        foreach (bool thread in Threads.Skip(1))
                        {
                            if (thread)
                            {
                                cont = true;
                            }
                        }
                        if (!cont)
                        {
                            contextSwitch = false;
                        }
                    }
                }
                #endregion

                //if (debug)
                //{
                //    byte* p = mem;
                //    Console.WriteLine("\n\nByte Table:");
                //    for (int i = 50000; i < MEM_SIZE; i++)
                //    {
                //        if (char.IsLetter((char)*p))
                //        {
                //            Console.WriteLine("{0}: {1}", i, (char)*p);
                //        }
                //        else
                //        {
                //            Console.WriteLine("{0}: {1}", i, *p);
                //        }
                //        p++;
                //    }
                //    Console.WriteLine();
                //}
                if (debug)
                {
                    Console.Write("\n\nPress Any Key To Continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
