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
        static Dictionary<string, int> OC = new Dictionary<string, int>();
        static Dictionary<int, string> ROC = new Dictionary<int, string>();
        static Dictionary<string, int> DT = new Dictionary<string, int>();

        static int PC;
        static int printVariable;
        static Dictionary<string, int> ST = new Dictionary<string, int>();
        static Dictionary<int, string> RST = new Dictionary<int, string>();
        static Dictionary<string, int> SR = new Dictionary<string, int>();
        static bool[] Threads = new bool[5];
        const int MEM_SIZE = 100000;
        const int ThreadStackSize = 1000;

        unsafe static void Main(string[] args)
        {
            bool debug = false;
            try
            {
                #region Fill OpCodes
                OC.Add("JMP", 1);
                OC.Add("JMR", 2);
                OC.Add("BNZ", 3);
                OC.Add("BGT", 4);
                OC.Add("BLT", 5);
                OC.Add("BRZ", 6);
                OC.Add("MOV", 7);
                OC.Add("LDA", 8);
                OC.Add("STR", 9);
                OC.Add("LDR", 10);
                OC.Add("STB", 11);
                OC.Add("LDB", 12);
                OC.Add("ADD", 13);
                OC.Add("ADI", 14);
                OC.Add("SUB", 15);
                OC.Add("MUL", 16);
                OC.Add("DIV", 17);
                OC.Add("AND", 18);
                OC.Add("OR", 19);
                OC.Add("CMP", 20);
                OC.Add("TRP", 21);
                OC.Add("RUN", 26);
                OC.Add("END", 27);
                OC.Add("BLK", 28);
                OC.Add("LCK", 29);
                OC.Add("ULK", 30);

                foreach (var item in OC)
                {
                    ROC.Add(item.Value, item.Key);
                }
                ROC.Add(22, "STR");
                ROC.Add(23, "LDR");
                ROC.Add(24, "STB");
                ROC.Add(25, "LDB");
                #endregion

                #region Fill Directives
                DT.Add(".INT", 4);
                DT.Add(".BYT", 1);
                #endregion

                #region Load Threads
                Threads[0] = true;
                Threads[1] = false;
                Threads[2] = false;
                Threads[3] = false;
                Threads[4] = false;
                #endregion

                #region Fill Special Registers
                SR.Add("PC", 8);
                SR.Add("SP", 9);
                SR.Add("FP", 10);
                SR.Add("SL", 11);
                SR.Add("SB", 12);
                #endregion

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
                int[] RT = new int[8 + SR.Count];
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
                                if (OC.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else if (DT.ContainsKey(word))
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
                                if (OC.ContainsKey(word))
                                {
                                    opCode = word;
                                    counter++;
                                }
                                else if (DT.ContainsKey(word))
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
                    if (opCode != null && OC.ContainsKey(opCode))
                    {
                        if (!startProgram)
                        {
                            PC = locationCounter;
                            startProgram = true;
                        }
                        if (label != null && !ST.ContainsKey(label))
                        {
                            ST.Add(label, locationCounter);
                        }
                        //locationCounter += 12;
                        locationCounter += 12;
                    }
                    else if (opCode != null && DT.ContainsKey(opCode))
                    {
                        if (label != null && !ST.ContainsKey(label))
                        {
                            ST.Add(label, locationCounter);
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
                RT[SR["SL"]] = Convert.ToInt32(Math.Ceiling((Convert.ToDouble(locationCounter) - 12) / 4) * 4) + 4;
                RT[SR["SB"]] = MEM_SIZE - 8;
                RT[SR["SP"]] = MEM_SIZE - 8;
                foreach (var item in ST)
                {
                    RST.Add(item.Value, item.Key);
                }
                sr.Close();

                if (debug)
                {
                    Console.WriteLine("Symbol Table:");
                    foreach (var value in ST)
                    {
                        Console.WriteLine("{0}->{1}", value.Key, value.Value);
                    }
                }
                #endregion

                #region Load Byte Code
                int cError = 0;
                byte* mem = stackalloc byte[MEM_SIZE];
                byte* newP2 = &mem[PC];
                int currValue = 0;
                try
                {
                    printVariable = PC;
                    sr = File.OpenText(args[0]);
                    while ((input = sr.ReadLine()) != null)
                    {
                        cError++;
                        if (cError == 205)
                        { }
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
                                    if (OC.ContainsKey(word))
                                    {
                                        label = word;
                                        opCode = word;
                                        counter++;
                                    }
                                    else if (DT.ContainsKey(word))
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
                                    if (OC.ContainsKey(word))
                                    {
                                        opCode = word;
                                        counter++;
                                    }
                                    else if (DT.ContainsKey(word))
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
                                    if (OC.ContainsKey(opCode) && !int.TryParse(word, out int value))
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
                        if (opCode != null && OC.ContainsKey(opCode))
                        {
                            if ((opCode == "LDR" || opCode == "STR" || opCode == "STB" || opCode == "LDB") && ((op2.Length == 2 && op2[0] == 'R') || SR.ContainsKey(op2)))
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
                                int value = OC[opCode];
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
                            else if (SR.ContainsKey(op1))
                            {
                                byte[] array = BitConverter.GetBytes(SR[op1]);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                            else if (ST.ContainsKey(op1))
                            {
                                int value = ST[op1];
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
                            else if (SR.ContainsKey(op2))
                            {
                                byte[] array = BitConverter.GetBytes(SR[op2]);
                                foreach (var bit in array)
                                {
                                    *newP2 = bit;
                                    newP2++;
                                }
                            }
                            else if (ST.ContainsKey(op2))
                            {
                                int value = ST[op2];
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
                        else if (opCode != null && DT.ContainsKey(opCode))
                        {
                            byte* newP;
                            if (label != null)
                            {
                                if (!ST.ContainsKey(label))
                                {
                                    Console.WriteLine("Label Not Added To Symbol Table.");
                                    break;
                                }
                                currValue = ST[label];
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
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Loading Byte Code:\n" + ex.Message + "\n\nError Line: " + cError);
                    Console.ReadKey();
                }
                #endregion

                #region Run Assembly Code
                byte* currP;
                int intOp1 = 0;
                int intOp2 = 0;
                int printValue = 0;
                RT[SR["PC"]] = PC;
                int newThreadID = -1;
                int newThreadPC = -1;
                int countyer = 0;
                bool running = true;
                byte[] printArray = new byte[4];
                byte[] opArray = new byte[4];
                byte[] op1Array = new byte[4];
                byte[] op2Array = new byte[4];
                bool breakPoint = false;
                int whileCount = 0;
                debug = false;
                while (running)
                {
                    if (debug)
                    {
                        byte* p = mem;
                        StreamWriter sw1 = new StreamWriter("CurrentStack.txt");
                        sw1.WriteLine("\n\nByte Table:");
                        for (int i = 0; i < MEM_SIZE; i += 4)
                        {
                            if (i == 19940)
                            { }
                            for (int j = 0; j < 4; j++)
                            {
                                printArray[j] = *p;
                                p++;
                            }
                            printValue = BitConverter.ToInt32(printArray, 0);
                            sw1.WriteLine("{0}: {1}", i, printValue);
                            //p++;
                        }
                        sw1.WriteLine();
                        sw1.Close();
                    }

                    PC = RT[SR["PC"]];
                    if(PC == 880)
                    { }
                    currP = &mem[PC];
                    for (int i = 0; i < 4; i++)
                    {
                        opArray[i] = *currP;
                        currP++;
                    }
                    int opCodeInt = BitConverter.ToInt32(opArray, 0);
                    if (ROC.ContainsKey(opCodeInt))
                    {
                        opCode = ROC[opCodeInt];
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
                        if(intOp1 == ST["PROGRAMEND"])
                        {
                            //debug = true;
                        }
                        if(breakPoint)
                        {
                            if (whileCount == 0)
                            {
                                debug = true;
                            }
                            whileCount++;
                            breakPoint = false;
                        }
                        if(RT[0] == 99968 || RT[1] == 99968 || RT[2] == 99968 || RT[3] == 99968 || RT[4] == 99968 || RT[5] == 99968 || RT[6] == 99968)
                        {
                            if (whileCount == 3)
                            {
                                debug = true;
                            }
                        }

                        switch (opCode)
                        {
                            case "JMP":
                                if (RST.ContainsKey(intOp1))
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
                                if (RT[intOp1] != 0)
                                {
                                    if (RST.ContainsKey(intOp2))
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
                                if (RT[intOp1] > 0)
                                {
                                    if (RST.ContainsKey(intOp2))
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
                                if (RT[intOp1] < 0)
                                {
                                    if (RST.ContainsKey(intOp2))
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
                                if (RT[intOp1] == 0)
                                {
                                    if (RST.ContainsKey(intOp2))
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
                                RT[intOp1] = RT[intOp2];
                                if(intOp1 == 8)
                                {
                                    PC = RT[intOp1];
                                }
                                break;
                            case "LDA":
                                if (RST.ContainsKey(intOp2))
                                {
                                    RT[intOp1] = intOp2;
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
                                    byte[] bty = BitConverter.GetBytes(RT[intOp1]);
                                    if (bty.Length == 2)
                                    {
                                        length = 1;
                                    }
                                    for (int i = 0; i < length; i++)
                                    {
                                        mem[RT[intOp2] + i] = bty[i];
                                    }
                                    bty = null;
                                }
                                else
                                {
                                    int length = 4;
                                    byte[] bty = BitConverter.GetBytes(RT[intOp1]);
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
                                    byte[] bty = { mem[RT[intOp2]], mem[RT[intOp2] + 1],
                                    mem[RT[intOp2] + 2], mem[RT[intOp2] + 3] };
                                    RT[intOp1] = BitConverter.ToInt32(bty, 0);
                                    if (intOp1 == 8)
                                    {
                                        PC = RT[intOp1];
                                    }
                                    bty = null;
                                }
                                else
                                {
                                    byte[] bty = { mem[intOp2], mem[intOp2 + 1], mem[intOp2 + 2], mem[intOp2 + 3] };
                                    RT[intOp1] = BitConverter.ToInt32(bty, 0);
                                    bty = null;
                                }
                                break;
                            case "STB":
                                if (opCodeInt == 24)
                                {
                                    byte[] stb = BitConverter.GetBytes(RT[intOp1]);
                                    mem[RT[intOp2]] = stb[0];
                                    stb = null;
                                }
                                else
                                {
                                    byte[] stb = BitConverter.GetBytes(RT[intOp1]);
                                    mem[intOp2] = stb[0];
                                    stb = null;
                                }
                                break;
                            case "LDB":
                                if (opCodeInt == 25)
                                {
                                    byte[] ldb = BitConverter.GetBytes(mem[RT[intOp2]]);
                                    RT[intOp1] = ldb[0];
                                    ldb = null;
                                }
                                else
                                {
                                    byte[] ldb = BitConverter.GetBytes(mem[intOp2]);
                                    RT[intOp1] = ldb[0];
                                    ldb = null;
                                }
                                break;
                            case "ADD":
                                RT[intOp1] += RT[intOp2];
                                break;
                            case "ADI":
                                RT[intOp1] += intOp2;
                                break;
                            case "SUB":
                                RT[intOp1] -= RT[intOp2];
                                break;
                            case "MUL":
                                RT[intOp1] *= RT[intOp2];
                                break;
                            case "DIV":
                                RT[intOp1] /= RT[intOp2];
                                break;
                            case "AND":
                                if(RT[intOp1] == 1 && RT[intOp2] == 1)
                                {
                                    RT[intOp1] = 1;
                                }
                                else
                                {
                                    RT[intOp1] = 0;
                                }
                                break;
                            case "OR":
                                if (RT[intOp1] == 1 || RT[intOp2] == 1)
                                {
                                    RT[intOp1] = 1;
                                }
                                else
                                {
                                    RT[intOp1] = 0;
                                }
                                break;
                            case "RUN":
                                bool found = false;
                                for (int i = 1; i < Threads.Length; i++)
                                {
                                    if (!Threads[i])
                                    {
                                        RT[intOp1] = i;
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
                                if (RT[intOp1] == RT[intOp2])
                                {
                                    RT[intOp1] = 0;
                                }
                                else if (RT[intOp1] > RT[intOp2])
                                {
                                    RT[intOp1] = 1;
                                }
                                else if (RT[intOp1] < RT[intOp2])
                                {
                                    RT[intOp1] = -1;
                                }
                                break;
                            case "TRP":
                                switch (intOp1)
                                {
                                    case 0:
                                        running = false;
                                        break;
                                    case 1:
                                        Console.Write("{0}", (int)RT[3]);
                                        break;
                                    case 2:
                                        RT[3] = int.Parse(Console.ReadLine());
                                        break;
                                    case 3:
                                        char c = (char)RT[3];
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
                                        var value = Console.ReadLine()[0];
                                        RT[3] = value;
                                        break;
                                    case 100:
                                        breakPoint = true;
                                        break;
                                    default:
                                        running = false;
                                        break;
                                }
                                break;
                        }
                    }
                    PC += 12;
                    RT[SR["PC"]] = PC;
                    if (RT[SR["SP"]] < RT[SR["SL"]])
                    {
                        Console.WriteLine("Stack Overflow Occured!!!");
                        break;
                    }
                    if (RT[SR["SP"]] > RT[SR["SB"]])
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
                            for (int i = 0; i < RT.Length; i++)
                            {
                                int memLocation = 0;
                                if (i == SR["PC"])
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
                                else if (i == SR["SP"])
                                {
                                    memLocation = threadSB - 40;
                                }
                                else if (i == SR["FP"])
                                {
                                    memLocation = threadSB - 44;
                                }
                                else if (i == SR["SL"])
                                {
                                    memLocation = threadSB - 48;
                                }
                                else if (i == SR["SB"])
                                {
                                    memLocation = threadSB - 52;
                                }
                                int length = 4;
                                byte[] bty = BitConverter.GetBytes(RT[i]);
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
                                RT[SR["PC"]] = newThreadPC;
                                PC = newThreadPC;
                                newThreadPC = -1;
                                RT[SR["SP"]] = threadSB - 56;
                                RT[SR["FP"]] = threadSB - 56;
                                RT[SR["SL"]] = threadSB - ThreadStackSize;
                                RT[SR["SB"]] = threadSB;
                                RT[0] = 0;
                                RT[1] = 0;
                                RT[2] = 0;
                                RT[3] = 0;
                                RT[4] = 0;
                                RT[5] = 0;
                                RT[6] = 0;
                                RT[7] = 0;
                            }
                            else
                            {
                                threadSB -= 4; // PC
                                byte[] pcbty = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[SR["PC"]] = BitConverter.ToInt32(pcbty, 0);
                                PC = RT[SR["PC"]];
                                threadSB -= 4; // 0
                                pcbty = null;
                                byte[] bty0 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[0] = BitConverter.ToInt32(bty0, 0);
                                bty0 = null;
                                threadSB -= 4; // 1
                                byte[] bty1 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[1] = BitConverter.ToInt32(bty1, 0);
                                bty1 = null;
                                threadSB -= 4; // 2
                                byte[] bty2 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[2] = BitConverter.ToInt32(bty2, 0);
                                bty2 = null;
                                threadSB -= 4; // 3
                                byte[] bty3 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[3] = BitConverter.ToInt32(bty3, 0);
                                bty3 = null;
                                threadSB -= 4; // 4
                                byte[] bty4 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[4] = BitConverter.ToInt32(bty4, 0);
                                bty4 = null;
                                threadSB -= 4; // 5
                                byte[] bty5 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[5] = BitConverter.ToInt32(bty5, 0);
                                bty5 = null;
                                threadSB -= 4; // 6
                                byte[] bty6 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[6] = BitConverter.ToInt32(bty6, 0);
                                bty6 = null;
                                threadSB -= 4; // 7
                                byte[] bty7 = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[7] = BitConverter.ToInt32(bty7, 0);
                                bty7 = null;
                                threadSB -= 4; // SP
                                byte[] btysp = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[SR["SP"]] = BitConverter.ToInt32(btysp, 0);
                                btysp = null;
                                threadSB -= 4; // FP
                                byte[] btyfp = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[SR["FP"]] = BitConverter.ToInt32(btyfp, 0);
                                btyfp = null;
                                threadSB -= 4; // SL
                                byte[] btysl = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[SR["SL"]] = BitConverter.ToInt32(btysl, 0);
                                btysl = null;
                                threadSB -= 4; // SL
                                byte[] btysb = { mem[threadSB], mem[threadSB + 1], mem[threadSB + 2], mem[threadSB + 3] };
                                RT[SR["SB"]] = BitConverter.ToInt32(btysb, 0);
                                btysb = null;
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

                //byte* p2 = mem;
                //StreamWriter sw = new StreamWriter("CurrentStack.txt");
                //sw.WriteLine("\n\nByte Table:");
                //for (int i = 0; i < MEM_SIZE; i += 4)
                //{
                //    if (i == 19940)
                //    { }
                //    for (int j = 0; j < 4; j++)
                //    {
                //        printArray[j] = *p2;
                //        p2++;
                //    }
                //    printValue = BitConverter.ToInt32(printArray, 0);
                //    sw.WriteLine("{0}: {1}", i, printValue);
                //    //p++;
                //}
                //sw.WriteLine();
                //sw.Close();
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
                //if (debug)
                //{
                //    Console.WriteLine(ex);
                //    Console.ReadKey();
                //}
            }
        }
    }
}
