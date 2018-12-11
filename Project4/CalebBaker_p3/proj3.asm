﻿SIZE .INT 7
cnt .INT 0
tenth .INT 0
c .INT 0
  .INT 0
  .INT 0
  .INT 0
  .INT 0
  .INT 0
  .INT 0
data .INT 0
flag .INT 0
opdv .INT 0
S .BYT 'S'
T .BYT 'T'
A .BYT 'A'
C .BYT 'C'
K .BYT 'K'
space .BYT '`'
NL .BYT '\n'
Enter .BYT '\r'
O .BYT 'O'
V .BYT 'V'
E .BYT 'E'
R .BYT 'R'
F .BYT 'F'
L .BYT 'L'
W .BYT 'W'
U .BYT 'U'
N .BYT 'N'
D .BYT 'D'
u .BYT 'u'
m .BYT 'm'
b .BYT 'b'
e .BYT 'e'
r .BYT 'r'
t .BYT 't'
o .BYT 'o'
B .BYT 'B'
i .BYT 'i'
g .BYT 'g'
s .BYT 's'
n .BYT 'n'
a .BYT 'a'
p .BYT 'p'
d .BYT 'd'
at .BYT '@'
Min .BYT '-'
Pos .BYT '+'
I .INT 2
J .INT 5
zero .INT 0
MOV R5 SP // Start of Main()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP main
opd LDR R0 zero
STR R0 SP
ADI SP -4
LDR R1 FP
ADI R1 -16
LDR R2 R1
LDR R1 FP
ADI R1 -20
LDR R3 R1
LDR R4 zero
MOV R5 R2
CMP R5 R4
BNZ R5 Else1
STR R4 R3
JMP opdIf2
Else1 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else2
STR R4 R3
JMP opdIf2
Else2 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else3
STR R4 R3
JMP opdIf2
Else3 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else4
STR R4 R3
JMP opdIf2
Else4 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else5
STR R4 R3
JMP opdIf2
Else5 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else6
STR R4 R3
JMP opdIf2
Else6 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else7
STR R4 R3
JMP opdIf2
Else7 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else8
STR R4 R3
JMP opdIf2
Else8 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else9
STR R4 R3
JMP opdIf2
Else9 ADI R4 1
MOV R5 R2
CMP R5 R4
BNZ R5 Else10
STR R4 R3
JMP opdIf2
Else10 MOV R3 R2
TRP 3
LDB R3 space
TRP 3
LDB R3 i
TRP 3
LDB R3 s
TRP 3
LDB R3 space
TRP 3
LDB R3 n
TRP 3
LDB R3 o
TRP 3
LDB R3 t
TRP 3
LDB R3 space
TRP 3
LDB R3 a
TRP 3
LDB R3 space
TRP 3
LDB R3 n
TRP 3
LDB R3 u
TRP 3
LDB R3 m
TRP 3
LDB R3 b
TRP 3
LDB R3 e
TRP 3
LDB R3 r
TRP 3
LDB R3 NL
TRP 3
LDR R4 zero
ADI R4 1
STR R4 flag
opdIf2 LDR R0 flag
LDR R4 zero
CMP R0 R4
BNZ R0 EndopdIf2
LDR R1 FP // Load s
ADI R1 -8
LDR R2 R1 // Load s value
LDB R3 Pos
CMP R2 R3
BNZ R2 ElseIf3
LDR R5 FP // Load k
ADI R5 -12
LDR R6 R5 // Load k value
MUL R3 R6
JMP EndIf3
ElseIf3 LDR R5 FP
ADI R5 -12
LDR R6 R5
MOV R7 R6
SUB R6 R7
SUB R6 R7
MUL R3 R6
EndIf3 LDR R0 opdv
ADD R0 R3
STR R0 opdv
EndopdIf2 MOV SP FP // Deallocate
MOV R5 SP
CMP R5 SB
BGT R5 UNDERFLOW
LDR R5 FP
LDR R6 FP
ADI R6 -4
LDR FP R6
ADI R5 -12
LDR PC R5
flush LDR R4 zero
STR R4 data
LDA R7 c
TRP 4
MOV R7 R3
LDB R6 Enter
FlushWhile CMP R7 R6
BNZ R7 FlushWhile
MOV SP FP
MOV R5 SP
CMP R5 SB
BGT R5 UNDERFLOW
LDR R5 FP
LDR R6 FP
ADI R6 -4
LDR FP R6
LDR PC R5
reset LDR R0 zero
STR R0 SP
ADI SP -4
LDA R1 c
resetFor MOV R2 FP // load FP Address
ADI R2 -24 // add offset 24 for local variable
LDR R0 R2 // Load register 0 to value at local variable address
ADD R1 R0 // Add local variable to c address
LDR R4 zero // set register 4 to zero
STR R4 R1 // store register 4 to register 1 address to zero c[k] = 0;
LDR R6 SIZE // Load size variable
ADI R0 1
STR R0 R2
CMP R0 R6 // Compart local variable value to size
BLT R0 resetFor // Branch is previous value returns less than zero
MOV R2 FP
ADI R2 -8
LDR R3 R2
STR R3 data
MOV R2 FP
ADI R2 -12
LDR R3 R2
STR R3 opdv
MOV R2 FP
ADI R2 -16
LDR R3 R2
STR R3 cnt
MOV R2 FP
ADI R2 -20
LDR R3 R2
STR R3 flag
MOV SP FP // Deallocate
MOV R5 SP
CMP R5 SB
BGT R5 UNDERFLOW
MOV R5 FP
ADI R5 12
MOV R6 FP
ADI R6 -4
MOV FP R6
ADI R5 -12
LDR PC R5
getdata LDR R0 cnt
LDR R1 SIZE
CMP R0 R1
BGT R0 getDataELSE
BRZ R0 getDataELSE
LDA R2 c
LDR R0 cnt
ADD R2 R0
TRP 4
STR R3 R2
ADI R0 1
STR R0 cnt
JMP ENDGetDataElse
getDataELSE LDB R3 N
TRP 3
LDB R3 u
TRP 3
LDB R3 m
TRP 3
LDB R3 b
TRP 3
LDB R3 e
TRP 3
LDB R3 r
TRP 3
LDB R3 space
TRP 3
LDB R3 t
TRP 3
LDB R3 o
TRP 3
LDB R3 o
TRP 3
LDB R3 space
TRP 3
LDB R3 B
TRP 3
LDB R3 i
TRP 3
LDB R3 g
TRP 3
LDB R3 NL
TRP 3
MOV R5 SP // Start of Flush()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP flush
ENDGetDataElse MOV SP FP // Deallocate
MOV R5 SP
CMP R5 SB
BGT R5 UNDERFLOW
MOV R5 FP
MOV R6 FP
ADI R6 -4
MOV FP R6
LDR PC R5
main MOV R5 SP // Start of Reset()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
LDR R5 zero
ADI R5 1
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP reset
MOV R5 SP // Start of getdata()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP getdata
MainWhile LDR R2 c
LDB R0 at
CMP R0 R2
BRZ R0 EndMainWhile
MainIf LDB R0 Pos
CMP R0 R2
BNZ R0 MainIfElse
LDB R0 Min
CMP R0 R2
BNZ R0 MainIfElse
MOV R5 SP // Start of getdata()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP getdata
JMP InnerMainWhile
MainIfElse LDA R1 c
ADI R1 4
STR R0 R1
LDB R1 Pos
STR R1 c
LDR R4 cnt
ADI R4 1
STR R4 cnt
InnerMainWhile LDR R0 data
LDR R1 zero
CMP R1 R0
BRZ R1 EndInnerMainWhile
LDA R1 c
LDR R2 cnt
ADI R2 -1
ADD R1 R2
LDR R2 R1
LDB R5 Enter
CMP R5 R2
BNZ R5 InnerIfElse
LDR R4 zero
STR R4 data
ADI R4 1
STR R4 tenth
LDR R4 cnt
ADI R4 -2
STR R4 cnt
Inner2While LDR R5 flag
LDR R6 zero
CMP R5 R6
BNZ R5 EndInner2While
CMP R4 R6
BRZ R4 EndInner2While
MOV R5 SP // Start of opd()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
LDR R5 c
STR R5 SP
ADI SP -4
LDR R5 tenth
STR R5 SP
ADI SP -4
LDA R5 c
LDR R6 cnt
ADD R5 R6
LDR R6 R5
STR R6 SP
ADI SP -4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP opd
LDR R5 cnt
ADI R5 -1
STR R5 cnt
LDR R5 tenth
LDR R6 zero
ADI R6 10
MUL R5 R6
STR R5 tenth
JMP Inner2While
EndInner2While LDR R5 flag
LDR R6 zero
CMP R5 R6
BNZ R5 EndInnerMainWhile
LDB R3 O
TRP 3
LDB R3 p
TRP 3
LDB R3 e
TRP 3
LDB R3 r
TRP 3
LDB R3 a
TRP 3
LDB R3 n
TRP 3
LDB R3 d
TRP 3
LDB R3 space
TRP 3
LDB R3 i
TRP 3
LDB R3 s
TRP 3
LDB R3 space
TRP 3
LDB R3 opdv
TRP 1
LDB R3 NL
TRP 3
JMP EndInnerIfElse
InnerIfElse MOV R5 SP // Start of getdata()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP getdata
EndInnerIfElse JMP InnerMainWhile
EndInnerMainWhile MOV R5 SP // Start of Reset()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
LDR R5 zero
ADI R5 1
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
LDR R5 zero
STR R5 SP
ADI SP -4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP reset
MOV R5 SP // Start of getdata()
ADI R5 -8
CMP R5 SL
BLT R5 OVERFLOW
MOV R3 FP // PFP = FP
MOV FP SP // FP = SP
ADI SP -4 // SP - 4
STR R3 SP // Store PFP in SP space
ADI SP -4 // SP - 4
MOV R1 PC
ADI R1 48
STR R1 FP
JMP getdata
JMP MainWhile
EndMainWhile JMP END
OVERFLOW LDB R3 S
TRP 3
LDB R3 T
TRP 3
LDB R3 A
TRP 3
LDB R3 C
TRP 3
LDB R3 K
TRP 3
LDB R3 space
TRP 3
LDB R3 O
TRP 3
LDB R3 V
TRP 3
LDB R3 E
TRP 3
LDB R3 R
TRP 3
LDB R3 F
TRP 3
LDB R3 L
TRP 3
LDB R3 O
TRP 3
LDB R3 W
TRP 3
TRP 0
UNDERFLOW LDB R3 S
TRP 3
LDB R3 T
TRP 3
LDB R3 A
TRP 3
LDB R3 C
TRP 3
LDB R3 K
TRP 3
LDB R3 space
TRP 3
LDB R3 U
TRP 3
LDB R3 N
TRP 3
LDB R3 D
TRP 3
LDB R3 E
TRP 3
LDB R3 R
TRP 3
LDB R3 F
TRP 3
LDB R3 L
TRP 3
LDB R3 O
TRP 3
LDB R3 W
TRP 3
END TRP 0