using System;
using System.Collections.Generic;
using System.Text;

namespace GEM
{
    public class CPU
    {
        public delegate void opCode();

        #region Fields

        private MMU _mmu;
        private opCode[] _cbSet;

        #endregion

        #region Constructors

        public CPU(MMU mmu)
        {
            _mmu = mmu;
            InstructionSet = new opCode[]
            {
                // 0x00
                NOP,            LD_BC_d16,      LD__BC__A,      INC_BC,         INC_B,          DEC_B,          LD_B_d8,        RLCA,
                LD__a16__SP,    ADD_HL_BC,      LD_A__BC_,      DEC_BC,         INC_C,          DEC_C,          LD_C_d8,        RRCA,
                // 0x10
                STOP,           LD_DE_d16,      LD__DE__A,      INC_DE,         INC_D,          DEC_D,          LD_D_d8,        RLA,
                JR_r8,          ADD_HL_DE,      LD_A__DE_,      DEC_DE,         INC_E,          DEC_E,          LD_E_d8,        RRA,
                // 0x20
                JR_NZ_r8,       LD_HL_d16,      LDI__HL__A,     INC_HL,         INC_H,          DEC_H,          LD_H_d8,        DAA,
                JR_Z_r8,        ADD_HL_HL,      LDI_A__HL_,     DEC_HL,         INC_L,          DEC_L,          LD_L_d8,        CPL,
                // 0x30
                JR_NC_r8,       LD_SP_d16,      LDD__HL__A,     INC_SP,         INC__HL_,       DEC__HL_,       LD__HL__d8,     SCF,
                JR_C_r8,        ADD_HL_SP,      LDD_A__HL_,     DEC_SP,         INC_A,          DEC_A,          LD_A_d8,        CCF,
                // 0x40
                LD_B_B,         LD_B_C,         LD_B_D,         LD_B_E,         LD_B_H,         LD_B_L,         LD_B__HL_,      LD_B_A,
                LD_C_B,         LD_C_C,         LD_C_D,         LD_C_E,         LD_C_H,         LD_C_L,         LD_C__HL_,      LD_C_A,
                // 0x50
                LD_D_B,         LD_D_C,         LD_D_D,         LD_D_E,         LD_D_H,         LD_D_L,         LD_D__HL_,      LD_D_A,
                LD_E_B,         LD_E_C,         LD_E_D,         LD_E_E,         LD_E_H,         LD_E_L,         LD_E__HL_,      LD_E_A,
                // 0x60
                LD_H_B,         LD_H_C,         LD_H_D,         LD_H_E,         LD_H_H,         LD_H_L,         LD_H__HL_,      LD_H_A,
                LD_L_B,         LD_L_C,         LD_L_D,         LD_L_E,         LD_L_H,         LD_L_L,         LD_L__HL_,      LD_L_A,
                // 0x70
                LD__HL__B,      LD__HL__C,      LD__HL__D,      LD__HL__E,      LD__HL__H,      LD__HL__L,      HALT,           LD__HL__A,
                LD_A_B,         LD_A_C,         LD_A_D,         LD_A_E,         LD_A_H,         LD_A_L,         LD_A__HL_,      LD_A_A,
                // 0x80
                ADD_A_B,        ADD_A_C,        ADD_A_D,        ADD_A_E,        ADD_A_H,        ADD_A_L,        ADD_A__HL_,     ADD_A_A,
                ADC_A_B,        ADC_A_C,        ADC_A_D,        ADC_A_E,        ADC_A_H,        ADC_A_L,        ADC_A__HL_,     ADC_A_A,
                // 0x90
                SUB_B,          SUB_C,          SUB_D,          SUB_E,          SUB_H,          SUB_L,          SUB__HL_,       SUB_A,
                SBC_A_B,        SBC_A_C,        SBC_A_D,        SBC_A_E,        SBC_A_H,        SBC_A_L,        SBC_A__HL_,     SBC_A_A,
                // 0xA0
                AND_B,          AND_C,          AND_D,          AND_E,          AND_H,          AND_L,          AND__HL_,       AND_A,
                XOR_B,          XOR_C,          XOR_D,          XOR_E,          XOR_H,          XOR_L,          XOR__HL_,       XOR_A,
                // 0xB0
                OR_B,           OR_C,           OR_D,           OR_E,           OR_H,           OR_L,           OR__HL_,        OR_A,
                CP_B,           CP_C,           CP_D,           CP_E,           CP_H,           CP_L,           CP__HL_,        CP_A,
                // 0xC0
                RET_NZ,         POP_BC,         JP_NZ_a16,      JP_a16,         CALL_NZ_a16,    PUSH_BC,        ADD_A_d8,       RST_00H,
                RET_Z,          RET,            JP_Z_a16,       PREFIX,         CALL_Z_a16,     CALL_a16,       ADC_A_d8,       RST_08H,
                // 0xD0
                RET_NC,         POP_DE,         JP_NC_a16,      NOT_IMPL,       CALL_NC_a16,    PUSH_DE,        SUB_d8,         RST_10H,
                RET_C,          RETI,           JP_C_a16,       NOT_IMPL,       CALL_C_a16,     NOT_IMPL,       SBC_A_d8,       RST_18H,
                // 0xE0
                LDH__a8__A,     POP_HL,         LD__C__A,       NOT_IMPL,       NOT_IMPL,       PUSH_HL,        AND_d8,         RST_20H, 
                ADD_SP_r8,      JP_HL,          LD__a16__A,     NOT_IMPL,       NOT_IMPL,       NOT_IMPL,       XOR_d8,         RST_28H,
                // 0xF0
                LDH_A__a8_,     POP_AF,         LD_A__C_,       DI,             NOT_IMPL,       PUSH_AF,        OR_d8,          RST_30H,
                LDHL_SP_r8,     LD_SP_HL,       LD_A__a16_,     EI,             NOT_IMPL,       NOT_IMPL,       CP_d8,          RST_38H
            };
            _cbSet = new opCode[]
            {
                // 0x00
                RLC_B,          RLC_C,          RLC_D,          RLC_E,          RLC_H,          RLC_L,          RLC__HL_,       RLC_A,
                RRC_B,          RRC_C,          RRC_D,          RRC_E,          RRC_H,          RRC_L,          RRC__HL_,       RRC_A,
                // 0x10
                RL_B,           RL_C,           RL_D,           RL_E,           RL_H,           RL_L,           RL__HL_,        RL_A,
                RR_B,           RR_C,           RR_D,           RR_E,           RR_H,           RR_L,           RR__HL_,        RR_A,
                // 0x20
                SLA_B,          SLA_C,          SLA_D,          SLA_E,          SLA_H,          SLA_L,          SLA__HL_,       SLA_A,
                SRA_B,          SRA_C,          SRA_D,          SRA_E,          SRA_H,          SRA_L,          SRA__HL_,       SRA_A,
                // 0x30
                SWAP_B,         SWAP_C,         SWAP_D,         SWAP_E,         SWAP_H,         SWAP_L,         SWAP__HL_,      SWAP_A,
                SRL_B,          SRL_C,          SRL_D,          SRL_E,          SRL_H,          SRL_L,          SRL__HL_,       SRL_A,
                // 0x40
                BIT_0_B,        BIT_0_C,        BIT_0_D,        BIT_0_E,        BIT_0_H,        BIT_0_L,        BIT_0__HL_,     BIT_0_A,
                BIT_1_B,        BIT_1_C,        BIT_1_D,        BIT_1_E,        BIT_1_H,        BIT_1_L,        BIT_1__HL_,     BIT_1_A,
                // 0x50
                BIT_2_B,        BIT_2_C,        BIT_2_D,        BIT_2_E,        BIT_2_H,        BIT_2_L,        BIT_2__HL_,     BIT_2_A,
                BIT_3_B,        BIT_3_C,        BIT_3_D,        BIT_3_E,        BIT_3_H,        BIT_3_L,        BIT_3__HL_,     BIT_3_A,
                // 0x60
                BIT_4_B,        BIT_4_C,        BIT_4_D,        BIT_4_E,        BIT_4_H,        BIT_4_L,        BIT_4__HL_,     BIT_4_A,
                BIT_5_B,        BIT_5_C,        BIT_5_D,        BIT_5_E,        BIT_5_H,        BIT_5_L,        BIT_5__HL_,     BIT_5_A,
                // 0x70
                BIT_6_B,        BIT_6_C,        BIT_6_D,        BIT_6_E,        BIT_6_H,        BIT_6_L,        BIT_6__HL_,     BIT_6_A,
                BIT_7_B,        BIT_7_C,        BIT_7_D,        BIT_7_E,        BIT_7_H,        BIT_7_L,        BIT_7__HL_,     BIT_7_A,
                // 0x80
                RES_0_B,        RES_0_C,        RES_0_D,        RES_0_E,        RES_0_H,        RES_0_L,        RES_0__HL_,     RES_0_A,
                RES_1_B,        RES_1_C,        RES_1_D,        RES_1_E,        RES_1_H,        RES_1_L,        RES_1__HL_,     RES_1_A,
                // 0x90
                RES_2_B,        RES_2_C,        RES_2_D,        RES_2_E,        RES_2_H,        RES_2_L,        RES_2__HL_,     RES_2_A,
                RES_3_B,        RES_3_C,        RES_3_D,        RES_3_E,        RES_3_H,        RES_3_L,        RES_3__HL_,     RES_3_A,
                // 0xA0
                RES_4_B,        RES_4_C,        RES_4_D,        RES_4_E,        RES_4_H,        RES_4_L,        RES_4__HL_,     RES_4_A,
                RES_5_B,        RES_5_C,        RES_5_D,        RES_5_E,        RES_5_H,        RES_5_L,        RES_5__HL_,     RES_5_A,
                // 0xB0
                RES_6_B,        RES_6_C,        RES_6_D,        RES_6_E,        RES_6_H,        RES_6_L,        RES_6__HL_,     RES_6_A,
                RES_7_B,        RES_7_C,        RES_7_D,        RES_7_E,        RES_7_H,        RES_7_L,        RES_7__HL_,     RES_7_A,
                // 0xC0
                SET_0_B,        SET_0_C,        SET_0_D,        SET_0_E,        SET_0_H,        SET_0_L,        SET_0__HL_,     SET_0_A,
                SET_1_B,        SET_1_C,        SET_1_D,        SET_1_E,        SET_1_H,        SET_1_L,        SET_1__HL_,     SET_1_A,
                // 0xD0
                SET_2_B,        SET_2_C,        SET_2_D,        SET_2_E,        SET_2_H,        SET_2_L,        SET_2__HL_,     SET_2_A,
                SET_3_B,        SET_3_C,        SET_3_D,        SET_3_E,        SET_3_H,        SET_3_L,        SET_3__HL_,     SET_3_A,
                // 0xE0
                SET_4_B,        SET_4_C,        SET_4_D,        SET_4_E,        SET_4_H,        SET_4_L,        SET_4__HL_,     SET_4_A,
                SET_5_B,        SET_5_C,        SET_5_D,        SET_5_E,        SET_5_H,        SET_5_L,        SET_5__HL_,     SET_5_A,
                // 0xF0
                SET_6_B,        SET_6_C,        SET_6_D,        SET_6_E,        SET_6_H,        SET_6_L,        SET_6__HL_,     SET_6_A,
                SET_7_B,        SET_7_C,        SET_7_D,        SET_7_E,        SET_7_H,        SET_7_L,        SET_7__HL_,     SET_7_A
            };
        }

        #endregion

        #region Properties

        public opCode[] InstructionSet { get; private set; }
        public int InstructionCycles { get; set; }  // clock cycles @ 4 Mhz
        public bool IsCPUHalt { get; set; }

        public Register A, B, C, D, E, H, L, F;
        public ushort AF                // A, F --> AF
        {
            get
            {
                return (ushort)((A << 8) + (F & 0xF0)); // lower 4 bit of F always zero
            }
            set
            {
                A = (byte)((value & 0xFF00) >> 8);
                F = (byte)(value & 0x00F0); // lower 4 bit of F always zero
            }
        }
        public ushort BC                // B, C --> BC
        {
            get
            {
                return (ushort)((B << 8) + C);
            }
            set
            {
                B = (byte)((value & 0xFF00) >> 8);
                C = (byte)(value & 0x00FF);
            }
        }
        public ushort DE                // D, E --> DE
        {
            get
            {
                return (ushort)((D << 8) + E);
            }
            set
            {
                D = (byte)((value & 0xFF00) >> 8);
                E = (byte)(value & 0x00FF);
            }
        }
        public ushort HL                // H, L --> HL
        {
            get
            {
                return (ushort)((H << 8) + L);
            }
            set
            {
                H = (byte)((value & 0xFF00) >> 8);
                L = (byte)(value & 0x00FF);
            }
        }
        public ushort SP { get; set; }  // Stack Pointer
        public ushort PC { get; set; }  // Programm Counter
        public int FlagZ                // Zero Flag
        {
            get { return F[7]; }
            set { F[7] = value; }
        }
        public int FlagN                // Subtract Flag
        {
            get { return F[6]; }
            set { F[6] = value; }
        }
        public int FlagH                // Half Carry Flag
        {
            get { return F[5]; }
            set { F[5] = value; }
        }
        public int FlagC                // Carry Flag
        {
            get { return F[4]; }
            set { F[4] = value; }
        }
        private byte d8
        {
            get { return _mmu.Read((ushort)(PC + 1)); }
        }
        private sbyte r8
        {
            get { return unchecked((sbyte)d8); }
        }
        private ushort d16              // AB, CD --> CDAB
        {
            get { return _mmu.ReadWord((ushort)(PC + 1)); }
        }
        #endregion

        #region Methods

        public void Reset()
        {
            PC = 0;
        }

        private void NOT_IMPL()
        {
            InstructionCycles = 4;
        }

        #region Misc / control instructions
        private void NOP()
        {
            // 0x00
            PC++;
            InstructionCycles = 4;
        }
        private void STOP()
        {
            // 0x10
            InstructionCycles = 4;
        }
        private void HALT()
        {
            // 0x76
            InstructionCycles = 4;
            IsCPUHalt = true;
        }
        private void PREFIX()
        {
            // 0xCB
            byte opCode = d8;
            _cbSet[opCode]();
            PC += 2;
        }
        private void DI()
        {
            // 0xF3
            _mmu.IME = false;
            PC++;
            InstructionCycles = 4;
        }
        private void EI()
        {
            // 0xFB
            _mmu.IME = true;
            PC++;
            InstructionCycles = 4;
        }
        #endregion

        #region Jumps / calls
        private void JR_r8()
        {
            // 0x18
            JR_r8_(true);
        }
        private void JR_NZ_r8()
        {
            // 0x20
            JR_r8_(FlagZ == 0);
        }
        private void JR_Z_r8()
        {
            // 0x28
            JR_r8_(FlagZ == 1);
        }
        private void JR_NC_r8()
        {
            // 0x30
            JR_r8_(FlagC == 0);
        }
        private void JR_C_r8()
        {
            // 0x38
            JR_r8_(FlagC == 1);
        }

        private void JP_a16()
        {
            // 0xC3
            JP_a16_(true);
        }
        private void JP_NZ_a16()
        {
            // 0xC2
            JP_a16_(FlagZ == 0);
        }
        private void JP_Z_a16()
        {
            // 0xCA
            JP_a16_(FlagZ == 1);
        }
        private void JP_NC_a16()
        {
            // 0xD2
            JP_a16_(FlagC == 0);
        }
        private void JP_C_a16()
        {
            // 0xDA
            JP_a16_(FlagC == 1);
        }
        private void JP_HL()
        {
            // 0xE9
            PC = HL;
            InstructionCycles = 4;
        }

        private void CALL_a16()
        {
            // 0xCD
            CALL_a16_(true);
        }
        private void CALL_NZ_a16()
        {
            // 0xC4
            CALL_a16_(FlagZ == 0);
        }
        private void CALL_Z_a16()
        {
            // 0xCC
            CALL_a16_(FlagZ == 1);
        }
        private void CALL_NC_a16()
        {
            // 0xD4
            CALL_a16_(FlagC == 0);
        }
        private void CALL_C_a16()
        {
            // 0xDC
            CALL_a16_(FlagC == 1);
        }

        private void RST_00H()
        {
            // 0xC7
            RST(0x00);
        }
        private void RST_08H()
        {
            // 0xCF
            RST(0x08);
        }
        private void RST_10H()
        {
            // 0xD7
            RST(0x10);
        }
        private void RST_18H()
        {
            // 0xDF
            RST(0x18);
        }
        private void RST_20H()
        {
            // 0xE7
            RST(0x20);
        }
        private void RST_28H()
        {
            // 0xEF
            RST(0x28);
        }
        private void RST_30H()
        {
            // 0xF7
            RST(0x30);
        }
        private void RST_38H()
        {
            // 0xFF
            RST(0x38);
        }

        private void RET()
        {
            // 0xC9
            PC = _mmu.ReadWord(SP);
            SP += 2;
            InstructionCycles = 16;
        }
        private void RETI()
        {
            // 0xD9
            PC = _mmu.ReadWord(SP);
            SP += 2;
            _mmu.IME = true;
            InstructionCycles = 16;
        }
        private void RET_NZ()
        {
            // 0xC0
            RET_(FlagZ == 0);
        }
        private void RET_Z()
        {
            // 0xC8
            RET_(FlagZ == 1);
        }
        private void RET_NC()
        {
            // 0xD0
            RET_(FlagC == 0);
        }
        private void RET_C()
        {
            // 0xD8
            RET_(FlagC == 1);
        }
        #endregion

        #region 8-bit load instructions
        private void LD__BC__A()
        {
            // 0x02
            _mmu.Write(BC, A);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__DE__A()
        {
            // 0x12
            _mmu.Write(DE, A);
            PC++;
            InstructionCycles = 8;
        }
        private void LDI__HL__A()
        {
            // 0x22
            _mmu.Write(HL, A);
            HL++;
            PC++;
            InstructionCycles = 8;
        }
        private void LDD__HL__A()
        {
            // 0x32
            _mmu.Write(HL, A);
            HL--;
            PC++;
            InstructionCycles = 8;
        }

        private void LD_A_d8()
        {
            // 0x3E
            A = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_B_d8()
        {
            // 0x06
            B = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_C_d8()
        {
            // 0x0E
            C = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_D_d8()
        {
            // 0x16
            D = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_E_d8()
        {
            // 0x1E
            E = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_H_d8()
        {
            // 0x26
            H = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD_L_d8()
        {
            // 0x2E
            L = d8;
            PC += 2;
            InstructionCycles = 8;
        }
        private void LD__HL__d8()
        {
            // 0x36
            _mmu.Write(HL, d8);
            PC += 2;
            InstructionCycles = 12;
        }

        private void LD_A__BC_()
        {
            // 0x0A
            A = _mmu.Read(BC);
            PC++;
            InstructionCycles = 8;
        }
        private void LD_A__DE_()
        {
            // 0x1A
            A = _mmu.Read(DE);
            PC++;
            InstructionCycles = 8;
        }
        private void LDI_A__HL_()
        {
            // 0x2A
            A = _mmu.Read(HL);
            HL++;
            PC++;
            InstructionCycles = 8;
        }
        private void LDD_A__HL_()
        {
            // 0x3A
            A = _mmu.Read(HL);
            HL--;
            PC++;
            InstructionCycles = 8;
        }

        private void LD_A_A()
        {
            // 0x7F
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_B()
        {
            // 0x78
            A = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_C()
        {
            // 0x79
            A = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_D()
        {
            // 0x7A
            A = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_E()
        {
            // 0x7B
            A = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_H()
        {
            // 0x7C
            A = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A_L()
        {
            // 0x7D
            A = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_A__HL_()
        {
            // 0x7E
            A = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_B_A()
        {
            // 0x47
            B = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_B()
        {
            // 0x40
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_C()
        {
            // 0x41
            B = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_D()
        {
            // 0x42
            B = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_E()
        {
            // 0x43
            B = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_H()
        {
            // 0x44
            B = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B_L()
        {
            // 0x45
            B = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_B__HL_()
        {
            // 0x46
            B = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_C_A()
        {
            // 0x4F
            C = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_B()
        {
            // 0x48
            C = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_C()
        {
            // 0x49
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_D()
        {
            // 0x4A
            C = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_E()
        {
            // 0x4B
            C = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_H()
        {
            // 0x4C
            C = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C_L()
        {
            // 0x4D
            C = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_C__HL_()
        {
            // 0x4E
            C = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_D_A()
        {
            // 0x57
            D = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_B()
        {
            // 0x50
            D = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_C()
        {
            // 0x51
            D = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_D()
        {
            // 0x52
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_E()
        {
            // 0x53
            D = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_H()
        {
            // 0x54
            D = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D_L()
        {
            // 0x55
            D = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_D__HL_()
        {
            // 0x56
            D = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_E_A()
        {
            // 0x5F
            E = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_B()
        {
            // 0x58
            E = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_C()
        {
            // 0x59
            E = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_D()
        {
            // 0x5A
            E = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_E()
        {
            // 0x5B
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_H()
        {
            // 0x5C
            E = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E_L()
        {
            // 0x5D
            E = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_E__HL_()
        {
            // 0x5E
            E = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_H_A()
        {
            // 0x67
            H = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_B()
        {
            // 0x60
            H = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_C()
        {
            // 0x61
            H = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_D()
        {
            // 0x62
            H = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_E()
        {
            // 0x63
            H = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_H()
        {
            // 0x64
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H_L()
        {
            // 0x65
            H = L;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_H__HL_()
        {
            // 0x66
            H = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD_L_A()
        {
            // 0x6F
            L = A;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_B()
        {
            // 0x68
            L = B;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_C()
        {
            // 0x69
            L = C;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_D()
        {
            // 0x6A
            L = D;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_E()
        {
            // 0x6B
            L = E;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_H()
        {
            // 0x6C
            L = H;
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L_L()
        {
            // 0x6D
            PC++;
            InstructionCycles = 4;
        }
        private void LD_L__HL_()
        {
            // 0x6E
            L = _mmu.Read(HL);
            PC++;
            InstructionCycles = 8;
        }

        private void LD__HL__A()
        {
            // 0x77
            _mmu.Write(HL, A);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__B()
        {
            // 0x70
            _mmu.Write(HL, B);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__C()
        {
            // 0x71
            _mmu.Write(HL, C);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__D()
        {
            // 0x72
            _mmu.Write(HL, D);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__E()
        {
            // 0x73
            _mmu.Write(HL, E);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__H()
        {
            // 0x74
            _mmu.Write(HL, H);
            PC++;
            InstructionCycles = 8;
        }
        private void LD__HL__L()
        {
            // 0x75
            _mmu.Write(HL, L);
            PC++;
            InstructionCycles = 8;
        }

        private void LDH__a8__A()
        {
            // 0xE0
            _mmu.Write((ushort)(0xFF00 + d8), A);
            PC += 2;
            InstructionCycles = 12;
        }
        private void LDH_A__a8_()
        {
            // 0xF0
            A = _mmu.Read((ushort)(0xFF00 + d8));
            PC += 2;
            InstructionCycles = 12;
        }

        private void LD__C__A()
        {
            // 0xE2
            _mmu.Write((ushort)(0xFF00 + C), A);
            PC++;
            InstructionCycles = 8;
        }
        private void LD_A__C_()
        {
            // 0xF2
            A = _mmu.Read((ushort)(0xFF00 + C));
            PC++;
            InstructionCycles = 8;
        }

        private void LD__a16__A()
        {
            // 0xEA
            _mmu.Write(d16, A);
            PC += 3;
            InstructionCycles = 16;
        }
        private void LD_A__a16_()
        {
            // 0xFA
            A = _mmu.Read(d16);
            PC += 3;
            InstructionCycles = 16;
        }
        #endregion

        #region 16-bit load instructions
        private void LD_BC_d16()
        {
            // 0x01
            BC = d16;
            PC += 3;
            InstructionCycles = 12;
        }
        private void LD_DE_d16()
        {
            // 0x11
            DE = d16;
            PC += 3;
            InstructionCycles = 12;
        }
        private void LD_HL_d16()
        {
            // 0x21
            HL = d16;
            PC += 3;
            InstructionCycles = 12;
        }
        private void LD_SP_d16()
        {
            // 0x31
            SP = d16;
            PC += 3;
            InstructionCycles = 12;
        }

        private void LD__a16__SP()
        {
            // 0x08
            _mmu.WriteWord(d16, SP);
            PC += 3;
            InstructionCycles = 20;
        }

        private void POP_AF()
        {
            // 0xF1
            AF = _mmu.ReadWord(SP);
            SP += 2;
            PC++;
            InstructionCycles = 12;
        }
        private void POP_BC()
        {
            // 0xC1
            BC = _mmu.ReadWord(SP);
            SP += 2;
            PC++;
            InstructionCycles = 12;
        }
        private void POP_DE()
        {
            // 0xD1
            DE = _mmu.ReadWord(SP);
            SP += 2;
            PC++;
            InstructionCycles = 12;
        }
        private void POP_HL()
        {
            // 0xE1
            HL = _mmu.ReadWord(SP);
            SP += 2;
            PC++;
            InstructionCycles = 12;
        }

        private void PUSH_AF()
        {
            // 0xF5
            SP -= 2;
            _mmu.WriteWord(SP, AF);
            PC++;
            InstructionCycles = 16;
        }
        private void PUSH_BC()
        {
            // 0xC5
            SP -= 2;
            _mmu.WriteWord(SP, BC);
            PC++;
            InstructionCycles = 16;
        }
        private void PUSH_DE()
        {
            // 0xD5
            SP -= 2;
            _mmu.WriteWord(SP, DE);
            PC++;
            InstructionCycles = 16;
        }
        private void PUSH_HL()
        {
            // 0xE5
            SP -= 2;
            _mmu.WriteWord(SP, HL);
            PC++;
            InstructionCycles = 16;
        }

        private void LDHL_SP_r8()
        {
            // 0xF8
            byte b = (byte)r8;
            FlagZ = 0;
            FlagN = 0;
            FlagH = flagH((byte)SP, b);
            FlagC = flagC((byte)SP + b);
            HL = (ushort)(SP + (sbyte)b);
            PC += 2;
            InstructionCycles = 12;
        }

        private void LD_SP_HL()
        {
            // 0xF9
            SP = HL;
            PC++;
            InstructionCycles = 8;
        }
        #endregion

        #region 8-bit arithmetic / logical instructions
        private void INC_A()
        {
            // 0x3C
            INC(ref A);
        }
        private void INC_B()
        {
            // 0x04
            INC(ref B);
        }
        private void INC_C()
        {
            // 0x0C
            INC(ref C);
        }
        private void INC_D()
        {
            // 0x14
            INC(ref D);
        }
        private void INC_E()
        {
            // 0x1C
            INC(ref E);
        }
        private void INC_H()
        {
            // 0x24
            INC(ref H);
        }
        private void INC_L()
        {
            // 0x2C
            INC(ref L);
        }
        private void INC__HL_()
        {
            // 0x34
            byte b = _mmu.Read(HL);
            FlagN = 0;
            FlagH = flagH(b, 1);
            b++;
            _mmu.Write(HL, b);
            FlagZ = flagZ(b);
            PC++;
            InstructionCycles = 12;
        }

        private void DEC_A()
        {
            // 0x3D
            DEC(ref A);
        }
        private void DEC_B()
        {
            // 0x05
            DEC(ref B);
        }
        private void DEC_C()
        {
            // 0x0D
            DEC(ref C);
        }
        private void DEC_D()
        {
            // 0x15
            DEC(ref D);
        }
        private void DEC_E()
        {
            // 0x1D
            DEC(ref E);
        }
        private void DEC_H()
        {
            // 0x25
            DEC(ref H);
        }
        private void DEC_L()
        {
            // 0x2D
            DEC(ref L);
        }
        private void DEC__HL_()
        {
            // 0x35
            byte b = _mmu.Read(HL);
            FlagN = 1;
            FlagH = flagHSub(b, 1);
            b--;
            _mmu.Write(HL, b);
            FlagZ = flagZ(b);
            PC++;
            InstructionCycles = 12;
        }

        private void DAA()
        {
            // 0x27
            byte correction = 0;
                
            // Use Correction
            if (FlagN == 0)
            {
                // Get Correction for Half Carry
                if (FlagH == 1 || (A & 0x0F) > 9)
                {
                    correction += 0x06;
                }
                FlagH = 0;
                // Get Correction for Carry
                if (FlagC == 1 || A > 0x99)
                {
                    correction += 0x60;
                    FlagC = 1;
                }
                else
                {
                    FlagC = 0;
                }
                A += correction;
            }
            else
            {
                // Get Correction for Half Carry
                if (FlagH == 1)
                {
                    correction += 0x06;
                }
                FlagH = 0;
                // Get Correction for Carry
                if (FlagC == 1)
                {
                    correction += 0x60;
                    FlagC = 1;
                }
                else
                {
                    FlagC = 0;
                }
                A -= correction;
            }
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }

        private void CPL()
        {
            // 0x2F
            FlagN = 1;
            FlagH = 1;
            /*
            byte cpl = 0;
            for (int i = 0; i < 8; i++)
            {
                cpl <<= 1;
                cpl |= (byte)(A & 1);
                A >>= 1;
            }
            A = cpl;
            */
            A = (byte)~A;
            PC++;
            InstructionCycles = 4;
        }

        private void SCF()
        {
            // 0x37
            FlagN = 0;
            FlagH = 0;
            FlagC = 1;
            PC++;
            InstructionCycles = 4;
        }

        private void CCF()
        {
            // 0x3F
            FlagN = 0;
            FlagH = 0;
            FlagC = (byte)(1 - FlagC);
            PC++;
            InstructionCycles = 4;
        }

        private void ADD_A_A()
        {
            // 0x87
            ADD(A);
        }
        private void ADD_A_B()
        {
            // 0x80
            ADD(B);
        }
        private void ADD_A_C()
        {
            // 0x81
            ADD(C);
        }
        private void ADD_A_D()
        {
            // 0x82
            ADD(D);
        }
        private void ADD_A_E()
        {
            // 0x83
            ADD(E);
        }
        private void ADD_A_H()
        {
            // 0x84
            ADD(H);
        }
        private void ADD_A_L()
        {
            // 0x85
            ADD(L);
        }
        private void ADD_A__HL_()
        {
            // 0x86
            byte b = _mmu.Read(HL);
            FlagN = 0;
            FlagH = flagH(A, b);
            FlagC = flagC(A + b);
            A += b;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 8;
        }

        private void ADC_A_A()
        {
            // 0x8F
            ADC(A);
        }
        private void ADC_A_B()
        {
            // 0x88
            ADC(B);
        }
        private void ADC_A_C()
        {
            // 0x89
            ADC(C);
        }
        private void ADC_A_D()
        {
            // 0x8A
            ADC(D);
        }
        private void ADC_A_E()
        {
            // 0x8B
            ADC(E);
        }
        private void ADC_A_H()
        {
            // 0x8C
            ADC(H);
        }
        private void ADC_A_L()
        {
            // 0x8D
            ADC(L);
        }
        private void ADC_A__HL_()
        {
            // 0x8E
            ADC(_mmu.Read(HL));
            InstructionCycles = 8;
        }

        private void SUB_A()
        {
            // 0x97
            SUB(A);
        }
        private void SUB_B()
        {
            // 0x90
            SUB(B);
        }
        private void SUB_C()
        {
            // 0x91
            SUB(C);
        }
        private void SUB_D()
        {
            // 0x92
            SUB(D);
        }
        private void SUB_E()
        {
            // 0x93
            SUB(E);
        }
        private void SUB_H()
        {
            // 0x94
            SUB(H);
        }
        private void SUB_L()
        {
            // 0x95
            SUB(L);
        }
        private void SUB__HL_()
        {
            //0x96
            byte b = _mmu.Read(HL);
            FlagN = 1;
            FlagH = flagHSub(A, b);
            FlagC = flagC(A - b);
            A -= b;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 8;
        }

        private void SBC_A_A()
        {
            // 0x9F
            SBC(A);
        }
        private void SBC_A_B()
        {
            // 0x98
            SBC(B);
        }
        private void SBC_A_C()
        {
            // 0x99
            SBC(C);
        }
        private void SBC_A_D()
        {
            // 0x9A
            SBC(D);
        }
        private void SBC_A_E()
        {
            // 0x9B
            SBC(E);
        }
        private void SBC_A_H()
        {
            // 0x9C
            SBC(H);
        }
        private void SBC_A_L()
        {
            // 0x9D
            SBC(L);
        }
        private void SBC_A__HL_()
        {
            // 0x9E
            SBC(_mmu.Read(HL));
            InstructionCycles = 8;
        }

        private void AND_A()
        {
            // 0xA7
            AND(A);
        }
        private void AND_B()
        {
            // 0xA0
            AND(B);
        }
        private void AND_C()
        {
            // 0xA1
            AND(C);
        }
        private void AND_D()
        {
            // 0xA2
            AND(D);
        }
        private void AND_E()
        {
            // 0xA3
            AND(E);
        }
        private void AND_H()
        {
            // 0xA4
            AND(H);
        }
        private void AND_L()
        {
            // 0xA5
            AND(L);
        }
        private void AND__HL_()
        {
            // 0xA6
            FlagN = 0;
            FlagH = 1;
            FlagC = 0;
            A &= _mmu.Read(HL);
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 8;
        }

        private void XOR_A()
        {
            // 0xAF
            XOR(A);
        }
        private void XOR_B()
        {
            // 0xA8
            XOR(B);
        }
        private void XOR_C()
        {
            // 0xA9
            XOR(C);
        }
        private void XOR_D()
        {
            // 0xAA
            XOR(D);
        }
        private void XOR_E()
        {
            // 0xAB
            XOR(E);
        }
        private void XOR_H()
        {
            // 0xAC
            XOR(H);
        }
        private void XOR_L()
        {
            // 0xAD
            XOR(L);
        }
        private void XOR__HL_()
        {
            // 0xAE
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A ^= _mmu.Read(HL);
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 8;
        }

        private void OR_A()
        {
            // 0xB7
            OR(A);
        }
        private void OR_B()
        {
            // 0xB0
            OR(B);
        }
        private void OR_C()
        {
            // 0xB1
            OR(C);
        }
        private void OR_D()
        {
            // 0xB2
            OR(D);
        }
        private void OR_E()
        {
            // 0xB3
            OR(E);
        }
        private void OR_H()
        {
            // 0xB4
            OR(H);
        }
        private void OR_L()
        {
            // 0xB5
            OR(L);
        }
        private void OR__HL_()
        {
            // 0xB6
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A |= _mmu.Read(HL);
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 8;
        }

        private void CP_A()
        {
            // 0xBF
            CP(A);
        }
        private void CP_B()
        {
            // 0xB8
            CP(B);
        }
        private void CP_C()
        {
            // 0xB9
            CP(C);
        }
        private void CP_D()
        {
            // 0xBA
            CP(D);
        }
        private void CP_E()
        {
            // 0xBB
            CP(E);
        }
        private void CP_H()
        {
            // 0xBC
            CP(H);
        }
        private void CP_L()
        {
            // 0xBD
            CP(L);
        }
        private void CP__HL_()
        {
            // 0xBE
            byte b = _mmu.Read(HL);
            FlagN = 1;
            FlagH = flagHSub(A, b);
            FlagC = flagC(A - b);
            FlagZ = flagZ((byte)(A - b));
            PC++;
            InstructionCycles = 8;
        }

        private void ADD_A_d8()
        {
            // 0xC6
            byte b = d8;
            FlagN = 0;
            FlagH = flagH(A, b);
            FlagC = flagC(A + b);
            A += b;
            FlagZ = flagZ(A);
            PC += 2;
            InstructionCycles = 8;
        }
        private void ADC_A_d8()
        {
            // 0xCE
            ADC(d8);
            PC ++;
            InstructionCycles = 8;
        }
        private void SUB_d8()
        {
            //0xD6
            byte b = d8;
            FlagN = 1;
            FlagH = flagHSub(A, b);
            FlagC = flagC(A - b);
            A -= b;
            FlagZ = flagZ(A);
            PC += 2;
            InstructionCycles = 8;
        }
        private void SBC_A_d8()
        {
            // 0xDE
            SBC(d8);
            PC++;
            InstructionCycles = 8;
        }
        private void AND_d8()
        {
            // 0xE6
            FlagN = 0;
            FlagH = 1;
            FlagC = 0;
            A &= d8;
            FlagZ = flagZ(A);
            PC += 2;
            InstructionCycles = 8;
        }
        private void XOR_d8()
        {
            // 0xEE
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A ^= d8;
            FlagZ = flagZ(A);
            PC += 2;
            InstructionCycles = 8;
        }
        private void OR_d8()
        {
            // 0xF6
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A |= d8;
            FlagZ = flagZ(A);
            PC += 2;
            InstructionCycles = 8;
        }
        private void CP_d8()
        {
            // 0xFE
            byte b = d8;
            FlagN = 1;
            FlagH = flagHSub(A, b);
            FlagC = flagC(A - b);
            FlagZ = flagZ(A - b);
            PC += 2;
            InstructionCycles = 8;
        }
        #endregion

        #region 16-bit arithmetic / logical instructions
        private void INC_BC()
        {
            // 0x03
            BC++;
            PC++;
            InstructionCycles = 8;
        }
        private void INC_DE()
        {
            // 0x13
            DE++;
            PC++;
            InstructionCycles = 8;
        }
        private void INC_HL()
        {
            // 0x23
            HL++;
            PC++;
            InstructionCycles = 8;
        }
        private void INC_SP()
        {
            // 0x33
            SP++;
            PC++;
            InstructionCycles = 8;
        }

        private void DEC_BC()
        {
            // 0x0B
            BC--;
            PC++;
            InstructionCycles = 8;
        }
        private void DEC_DE()
        {
            // 0x1B
            DE--;
            PC++;
            InstructionCycles = 8;
        }
        private void DEC_HL()
        {
            // 0x2B
            HL--;
            PC++;
            InstructionCycles = 8;
        }
        private void DEC_SP()
        {
            // 0x3B
            SP--;
            PC++;
            InstructionCycles = 8;
        }

        private void ADD_HL_BC()
        {
            // 0x09
            ADD_HL(BC);
        }
        private void ADD_HL_DE()
        {
            // 0x19
            ADD_HL(DE);
        }
        private void ADD_HL_HL()
        {
            // 0x29
            ADD_HL(HL);
        }
        private void ADD_HL_SP()
        {
            // 0x39
            ADD_HL(SP);
        }

        private void ADD_SP_r8()
        {
            // 0xE8
            byte b = (byte)r8;
            FlagZ = 0;
            FlagN = 0;
            FlagH = flagH((byte)SP, b);
            FlagC = flagC((byte)SP + b);
            SP = (ushort)(SP + (sbyte)b);
            PC += 2;
            InstructionCycles = 16;
        }
        #endregion

        #region 8-bit shift, rotate and bit instructions
        private void RLCA()
        {
            // 0x07
            FlagZ = 0;
            FlagN = 0;
            FlagH = 0;
            int temp = ((A << 1) | (A >> 7));
            A = (byte)(temp & 0xFF);
            FlagC = (byte)(temp >> 8);
            PC++;
            InstructionCycles = 4;
        }
        private void RLA()
        {
            // 0x17
            FlagZ = 0;
            FlagN = 0;
            FlagH = 0;
            int temp = ((A << 1) | FlagC);
            A = (byte)(temp & 0xFF);
            FlagC = (byte)(temp >> 8);
            PC++;
            InstructionCycles = 4;
        }
        private void RRCA()
        {
            // 0x0F
            FlagZ = 0;
            FlagN = 0;
            FlagH = 0;
            FlagC = (byte)(A & 1);
            A = (byte)((A >> 1) | ((A & 1) << 7));
            PC++;
            InstructionCycles = 4;
        }
        private void RRA()
        {
            // 0x1F
            FlagZ = 0;
            FlagN = 0;
            FlagH = 0;
            byte newC = (byte)(A & 1);
            A = (byte)((A >> 1) | (FlagC << 7));
            FlagC = newC;
            PC++;
            InstructionCycles = 4;
        }

        private void RLC_A()
        {
            // 0xCB 0x07
            RLC(ref A);
        }
        private void RLC_B()
        {
            // 0xCB 0x00
            RLC(ref B);
        }
        private void RLC_C()
        {
            // 0xCB 0x01
            RLC(ref C);
        }
        private void RLC_D()
        {
            // 0xCB 0x02
            RLC(ref D);
        }
        private void RLC_E()
        {
            // 0xCB 0x03
            RLC(ref E);
        }
        private void RLC_H()
        {
            // 0xCB 0x04
            RLC(ref H);
        }
        private void RLC_L()
        {
            // 0xCB 0x05
            RLC(ref L);
        }
        private void RLC__HL_()
        {
            // 0xCB 0x06
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            int temp = ((b << 1) | (b >> 7));
            b = (byte)(temp & 0xFF);
            _mmu.Write(HL, b);
            FlagC = (byte)(temp >> 8);
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void RRC_A()
        {
            // 0xCB 0x0F
            RRC(ref A);
        }
        private void RRC_B()
        {
            // 0xCB 0x08
            RRC(ref B);
        }
        private void RRC_C()
        {
            // 0xCB 0x09
            RRC(ref C);
        }
        private void RRC_D()
        {
            // 0xCB 0x0A
            RRC(ref D);
        }
        private void RRC_E()
        {
            // 0xCB 0x0B
            RRC(ref E);
        }
        private void RRC_H()
        {
            // 0xCB 0x0C
            RRC(ref H);
        }
        private void RRC_L()
        {
            // 0xCB 0x0D
            RRC(ref L);
        }
        private void RRC__HL_()
        {
            // 0xCB 0x0E
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            byte newC = (byte)(b & 1);
            b = (byte)((b >> 1) | ((b & 1) << 7));
            _mmu.Write(HL, b);
            FlagC = newC;
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void RL_A()
        {
            // 0xCB 0x17
            RL(ref A);
        }
        private void RL_B()
        {
            // 0xCB 0x10
            RL(ref B);
        }
        private void RL_C()
        {
            // 0xCB 0x11
            RL(ref C);
        }
        private void RL_D()
        {
            // 0xCB 0x12
            RL(ref D);
        }
        private void RL_E()
        {
            // 0xCB 0x13
            RL(ref E);
        }
        private void RL_H()
        {
            // 0xCB 0x14
            RL(ref H);
        }
        private void RL_L()
        {
            // 0xCB 0x15
            RL(ref L);
        }
        private void RL__HL_()
        {
            // 0xCB 0x16
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            int temp = ((b << 1) | FlagC);
            b = (byte)(temp & 0xFF);
            _mmu.Write(HL, b);
            FlagC = (byte)(temp >> 8);
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void RR_A()
        {
            // 0xCB 0x1F
            RR(ref A);
        }
        private void RR_B()
        {
            // 0xCB 0x18
            RR(ref B);
        }
        private void RR_C()
        {
            // 0xCB 0x19
            RR(ref C);
        }
        private void RR_D()
        {
            // 0xCB 0x1A
            RR(ref D);
        }
        private void RR_E()
        {
            // 0xCB 0x1B
            RR(ref E);
        }
        private void RR_H()
        {
            // 0xCB 0x1C
            RR(ref H);
        }
        private void RR_L()
        {
            // 0xCB 0x1D
            RR(ref L);
        }
        private void RR__HL_()
        {
            // 0xCB 0x1E
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            byte newC = (byte)(b & 1);
            b = (byte)((b >> 1) | (FlagC << 7));
            _mmu.Write(HL, b);
            FlagC = newC;
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void SLA_A()
        {
            // 0xCB 0x27
            SLA(ref A);
        }
        private void SLA_B()
        {
            // 0xCB 0x20
            SLA(ref B);
        }
        private void SLA_C()
        {
            // 0xCB 0x21
            SLA(ref C);
        }
        private void SLA_D()
        {
            // 0xCB 0x22
            SLA(ref D);
        }
        private void SLA_E()
        {
            // 0xCB 0x23
            SLA(ref E);
        }
        private void SLA_H()
        {
            // 0xCB 0x24
            SLA(ref H);
        }
        private void SLA_L()
        {
            // 0xCB 0x25
            SLA(ref L);
        }
        private void SLA__HL_()
        {
            // 0xCB 0x26
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            int temp = (b << 1);
            b = (byte)(temp & 0xFF);
            _mmu.Write(HL, b);
            FlagC = (byte)(temp >> 8);
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void SRA_A()
        {
            // 0xCB 0x2F
            SRA(ref A);
        }
        private void SRA_B()
        {
            // 0xCB 0x28
            SRA(ref B);
        }
        private void SRA_C()
        {
            // 0xCB 0x29
            SRA(ref C);
        }
        private void SRA_D()
        {
            // 0xCB 0x2A
            SRA(ref D);
        }
        private void SRA_E()
        {
            // 0xCB 0x2B
            SRA(ref E);
        }
        private void SRA_H()
        {
            // 0xCB 0x2C
            SRA(ref H);
        }
        private void SRA_L()
        {
            // 0xCB 0x2D
            SRA(ref L);
        }
        private void SRA__HL_()
        {
            // 0xCB 0x2E
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            byte newC = (byte)(b & 1);
            b = (byte)((b >> 1) | (b & 0b10000000));
            _mmu.Write(HL, b);
            FlagC = newC;
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void SWAP_A()
        {
            // 0xCB 0x37
            SWAP(ref A);
        }
        private void SWAP_B()
        {
            // 0xCB 0x30
            SWAP(ref B);
        }
        private void SWAP_C()
        {
            // 0xCB 0x31
            SWAP(ref C);
        }
        private void SWAP_D()
        {
            // 0xCB 0x32
            SWAP(ref D);
        }
        private void SWAP_E()
        {
            // 0xCB 0x33
            SWAP(ref E);
        }
        private void SWAP_H()
        {
            // 0xCB 0x34
            SWAP(ref H);
        }
        private void SWAP_L()
        {
            // 0xCB 0x35
            SWAP(ref L);
        }
        private void SWAP__HL_()
        {
            // 0xCB 0x36
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            byte b = _mmu.Read(HL);
            b = (byte)(((b & 0x0F) << 4) + ((b & 0xF0) >> 4));
            _mmu.Write(HL, b);
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void SRL_A()
        {
            // 0xCB 0x3F
            SRL(ref A);
        }
        private void SRL_B()
        {
            // 0xCB 0x38
            SRL(ref B);
        }
        private void SRL_C()
        {
            // 0xCB 0x39
            SRL(ref C);
        }
        private void SRL_D()
        {
            // 0xCB 0x3A
            SRL(ref D);
        }
        private void SRL_E()
        {
            // 0xCB 0x3B
            SRL(ref E);
        }
        private void SRL_H()
        {
            // 0xCB 0x3C
            SRL(ref H);
        }
        private void SRL_L()
        {
            // 0xCB 0x3D
            SRL(ref L);
        }
        private void SRL__HL_()
        {
            // 0xCB 0x3E
            FlagN = 0;
            FlagH = 0;
            byte b = _mmu.Read(HL);
            FlagC = (byte)(b & 1);
            b = (byte)(b >> 1);
            _mmu.Write(HL, b);
            FlagZ = flagZ(b);
            InstructionCycles = 16;
        }

        private void BIT_0_A()
        {
            // 0xCB 0x47
            BIT(0, A);
        }
        private void BIT_0_B()
        {
            // 0xCB 0x40
            BIT(0, B);
        }
        private void BIT_0_C()
        {
            // 0xCB 0x41
            BIT(0, C);
        }
        private void BIT_0_D()
        {
            // 0xCB 0x42
            BIT(0, D);
        }
        private void BIT_0_E()
        {
            // 0xCB 0x43
            BIT(0, E);
        }
        private void BIT_0_H()
        {
            // 0xCB 0x44
            BIT(0, H);
        }
        private void BIT_0_L()
        {
            // 0xCB 0x45
            BIT(0, L);
        }
        private void BIT_0__HL_()
        {
            // 0xCB 0x46
            BIT(0, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_1_A()
        {
            // 0xCB 0x4F
            BIT(1, A);
        }
        private void BIT_1_B()
        {
            // 0xCB 0x48
            BIT(1, B);
        }
        private void BIT_1_C()
        {
            // 0xCB 0x49
            BIT(1, C);
        }
        private void BIT_1_D()
        {
            // 0xCB 0x4A
            BIT(1, D);
        }
        private void BIT_1_E()
        {
            // 0xCB 0x4B
            BIT(1, E);
        }
        private void BIT_1_H()
        {
            // 0xCB 0x4C
            BIT(1, H);
        }
        private void BIT_1_L()
        {
            // 0xCB 0x4D
            BIT(1, L);
        }
        private void BIT_1__HL_()
        {
            // 0xCB 0x4E
            BIT(1, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_2_A()
        {
            // 0xCB 0x57
            BIT(2, A);
        }
        private void BIT_2_B()
        {
            // 0xCB 0x50
            BIT(2, B);
        }
        private void BIT_2_C()
        {
            // 0xCB 0x51
            BIT(2, C);
        }
        private void BIT_2_D()
        {
            // 0xCB 0x52
            BIT(2, D);
        }
        private void BIT_2_E()
        {
            // 0xCB 0x53
            BIT(2, E);
        }
        private void BIT_2_H()
        {
            // 0xCB 0x54
            BIT(2, H);
        }
        private void BIT_2_L()
        {
            // 0xCB 0x55
            BIT(2, L);
        }
        private void BIT_2__HL_()
        {
            // 0xCB 0x56
            BIT(2, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_3_A()
        {
            // 0xCB 0x5F
            BIT(3, A);
        }
        private void BIT_3_B()
        {
            // 0xCB 0x58
            BIT(3, B);
        }
        private void BIT_3_C()
        {
            // 0xCB 0x59
            BIT(3, C);
        }
        private void BIT_3_D()
        {
            // 0xCB 0x5A
            BIT(3, D);
        }
        private void BIT_3_E()
        {
            // 0xCB 0x5B
            BIT(3, E);
        }
        private void BIT_3_H()
        {
            // 0xCB 0x5C
            BIT(3, H);
        }
        private void BIT_3_L()
        {
            // 0xCB 0x5D
            BIT(3, L);
        }
        private void BIT_3__HL_()
        {
            // 0xCB 0x5E
            BIT(3, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_4_A()
        {
            // 0xCB 0x67
            BIT(4, A);
        }
        private void BIT_4_B()
        {
            // 0xCB 0x60
            BIT(4, B);
        }
        private void BIT_4_C()
        {
            // 0xCB 0x61
            BIT(4, C);
        }
        private void BIT_4_D()
        {
            // 0xCB 0x62
            BIT(4, D);
        }
        private void BIT_4_E()
        {
            // 0xCB 0x63
            BIT(4, E);
        }
        private void BIT_4_H()
        {
            // 0xCB 0x64
            BIT(4, H);
        }
        private void BIT_4_L()
        {
            // 0xCB 0x65
            BIT(4, L);
        }
        private void BIT_4__HL_()
        {
            // 0xCB 0x66
            BIT(4, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_5_A()
        {
            // 0xCB 0x6F
            BIT(5, A);
        }
        private void BIT_5_B()
        {
            // 0xCB 0x68
            BIT(5, B);
        }
        private void BIT_5_C()
        {
            // 0xCB 0x69
            BIT(5, C);
        }
        private void BIT_5_D()
        {
            // 0xCB 0x6A
            BIT(5, D);
        }
        private void BIT_5_E()
        {
            // 0xCB 0x6B
            BIT(5, E);
        }
        private void BIT_5_H()
        {
            // 0xCB 0x6C
            BIT(5, H);
        }
        private void BIT_5_L()
        {
            // 0xCB 0x6D
            BIT(5, L);
        }
        private void BIT_5__HL_()
        {
            // 0xCB 0x6E
            BIT(5, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_6_A()
        {
            // 0xCB 0x77
            BIT(6, A);
        }
        private void BIT_6_B()
        {
            // 0xCB 0x70
            BIT(6, B);
        }
        private void BIT_6_C()
        {
            // 0xCB 0x71
            BIT(6, C);
        }
        private void BIT_6_D()
        {
            // 0xCB 0x72
            BIT(6, D);
        }
        private void BIT_6_E()
        {
            // 0xCB 0x73
            BIT(6, E);
        }
        private void BIT_6_H()
        {
            // 0xCB 0x74
            BIT(6, H);
        }
        private void BIT_6_L()
        {
            // 0xCB 0x75
            BIT(6, L);
        }
        private void BIT_6__HL_()
        {
            // 0xCB 0x76
            BIT(6, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void BIT_7_A()
        {
            // 0xCB 0x7F
            BIT(7, A);
        }
        private void BIT_7_B()
        {
            // 0xCB 0x78
            BIT(7, B);
        }
        private void BIT_7_C()
        {
            // 0xCB 0x79
            BIT(7, C);
        }
        private void BIT_7_D()
        {
            // 0xCB 0x7A
            BIT(7, D);
        }
        private void BIT_7_E()
        {
            // 0xCB 0x7B
            BIT(7, E);
        }
        private void BIT_7_H()
        {
            // 0xCB 0x7C
            BIT(7, H);
        }
        private void BIT_7_L()
        {
            // 0xCB 0x7D
            BIT(7, L);
        }
        private void BIT_7__HL_()
        {
            // 0xCB 0x7E
            BIT(7, _mmu.Read(HL));
            InstructionCycles += 4; // =12
        }

        private void RES_0_A()
        {
            // 0xCB 0x87
            RES(0, ref A);
        }
        private void RES_0_B()
        {
            // 0xCB 0x80
            RES(0, ref B);
        }
        private void RES_0_C()
        {
            // 0xCB 0x81
            RES(0, ref C);
        }
        private void RES_0_D()
        {
            // 0xCB 0x82
            RES(0, ref D);
        }
        private void RES_0_E()
        {
            // 0xCB 0x83
            RES(0, ref E);
        }
        private void RES_0_H()
        {
            // 0xCB 0x84
            RES(0, ref H);
        }
        private void RES_0_L()
        {
            // 0xCB 0x85
            RES(0, ref L);
        }
        private void RES_0__HL_()
        {
            // 0xCB 0x86
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 0))));
            InstructionCycles = 16;
        }

        private void RES_1_A()
        {
            // 0xCB 0x8F
            RES(1, ref A);
        }
        private void RES_1_B()
        {
            // 0xCB 0x88
            RES(1, ref B);
        }
        private void RES_1_C()
        {
            // 0xCB 0x89
            RES(1, ref C);
        }
        private void RES_1_D()
        {
            // 0xCB 0x8A
            RES(1, ref D);
        }
        private void RES_1_E()
        {
            // 0xCB 0x8B
            RES(1, ref E);
        }
        private void RES_1_H()
        {
            // 0xCB 0x8C
            RES(1, ref H);
        }
        private void RES_1_L()
        {
            // 0xCB 0x8D
            RES(1, ref L);
        }
        private void RES_1__HL_()
        {
            // 0xCB 0x8E
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 1))));
            InstructionCycles = 16;
        }

        private void RES_2_A()
        {
            // 0xCB 0x97
            RES(2, ref A);
        }
        private void RES_2_B()
        {
            // 0xCB 0x90
            RES(2, ref B);
        }
        private void RES_2_C()
        {
            // 0xCB 0x91
            RES(2, ref C);
        }
        private void RES_2_D()
        {
            // 0xCB 0x92
            RES(2, ref D);
        }
        private void RES_2_E()
        {
            // 0xCB 0x93
            RES(2, ref E);
        }
        private void RES_2_H()
        {
            // 0xCB 0x94
            RES(2, ref H);
        }
        private void RES_2_L()
        {
            // 0xCB 0x95
            RES(2, ref L);
        }
        private void RES_2__HL_()
        {
            // 0xCB 0x96
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 2))));
            InstructionCycles = 16;
        }

        private void RES_3_A()
        {
            // 0xCB 0x9F
            RES(3, ref A);
        }
        private void RES_3_B()
        {
            // 0xCB 0x98
            RES(3, ref B);
        }
        private void RES_3_C()
        {
            // 0xCB 0x99
            RES(3, ref C);
        }
        private void RES_3_D()
        {
            // 0xCB 0x9A
            RES(3, ref D);
        }
        private void RES_3_E()
        {
            // 0xCB 0x9B
            RES(3, ref E);
        }
        private void RES_3_H()
        {
            // 0xCB 0x9C
            RES(3, ref H);
        }
        private void RES_3_L()
        {
            // 0xCB 0x9D
            RES(3, ref L);
        }
        private void RES_3__HL_()
        {
            // 0xCB 0x9E
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 3))));
            InstructionCycles = 16;
        }

        private void RES_4_A()
        {
            // 0xCB 0xA7
            RES(4, ref A);
        }
        private void RES_4_B()
        {
            // 0xCB 0xA0
            RES(4, ref B);
        }
        private void RES_4_C()
        {
            // 0xCB 0xA1
            RES(4, ref C);
        }
        private void RES_4_D()
        {
            // 0xCB 0xA2
            RES(4, ref D);
        }
        private void RES_4_E()
        {
            // 0xCB 0xA3
            RES(4, ref E);
        }
        private void RES_4_H()
        {
            // 0xCB 0xA4
            RES(4, ref H);
        }
        private void RES_4_L()
        {
            // 0xCB 0xA5
            RES(4, ref L);
        }
        private void RES_4__HL_()
        {
            // 0xCB 0xA6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 4))));
            InstructionCycles = 16;
        }

        private void RES_5_A()
        {
            // 0xCB 0xAF
            RES(5, ref A);
        }
        private void RES_5_B()
        {
            // 0xCB 0xA8
            RES(5, ref B);
        }
        private void RES_5_C()
        {
            // 0xCB 0xA9
            RES(5, ref C);
        }
        private void RES_5_D()
        {
            // 0xCB 0xAA
            RES(5, ref D);
        }
        private void RES_5_E()
        {
            // 0xCB 0xAB
            RES(5, ref E);
        }
        private void RES_5_H()
        {
            // 0xCB 0xAC
            RES(5, ref H);
        }
        private void RES_5_L()
        {
            // 0xCB 0xAD
            RES(5, ref L);
        }
        private void RES_5__HL_()
        {
            // 0xCB 0xAE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 5))));
            InstructionCycles = 16;
        }

        private void RES_6_A()
        {
            // 0xCB 0xB7
            RES(6, ref A);
        }
        private void RES_6_B()
        {
            // 0xCB 0xB0
            RES(6, ref B);
        }
        private void RES_6_C()
        {
            // 0xCB 0xB1
            RES(6, ref C);
        }
        private void RES_6_D()
        {
            // 0xCB 0xB2
            RES(6, ref D);
        }
        private void RES_6_E()
        {
            // 0xCB 0xB3
            RES(6, ref E);
        }
        private void RES_6_H()
        {
            // 0xCB 0xB4
            RES(6, ref H);
        }
        private void RES_6_L()
        {
            // 0xCB 0xB5
            RES(6, ref L);
        }
        private void RES_6__HL_()
        {
            // 0xCB 0xB6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 6))));
            InstructionCycles = 16;
        }

        private void RES_7_A()
        {
            // 0xCB 0xBF
            RES(7, ref A);
        }
        private void RES_7_B()
        {
            // 0xCB 0xB8
            RES(7, ref B);
        }
        private void RES_7_C()
        {
            // 0xCB 0xB9
            RES(7, ref C);
        }
        private void RES_7_D()
        {
            // 0xCB 0xBA
            RES(7, ref D);
        }
        private void RES_7_E()
        {
            // 0xCB 0xBB
            RES(7, ref E);
        }
        private void RES_7_H()
        {
            // 0xCB 0xBC
            RES(7, ref H);
        }
        private void RES_7_L()
        {
            // 0xCB 0xBD
            RES(7, ref L);
        }
        private void RES_7__HL_()
        {
            // 0xCB 0xBE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) & (byte)(0xFF - Math.Pow(2, 7))));
            InstructionCycles = 16;
        }

        private void SET_0_A()
        {
            // 0xCB 0xC7
            SET(0, ref A);
        }
        private void SET_0_B()
        {
            // 0xCB 0xC0
            SET(0, ref B);
        }
        private void SET_0_C()
        {
            // 0xCB 0xC1
            SET(0, ref C);
        }
        private void SET_0_D()
        {
            // 0xCB 0xC2
            SET(0, ref D);
        }
        private void SET_0_E()
        {
            // 0xCB 0xC3
            SET(0, ref E);
        }
        private void SET_0_H()
        {
            // 0xCB 0xC4
            SET(0, ref H);
        }
        private void SET_0_L()
        {
            // 0xCB 0xC5
            SET(0, ref L);
        }
        private void SET_0__HL_()
        {
            // 0xCB 0xC6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 0))));
            InstructionCycles = 16;
        }

        private void SET_1_A()
        {
            // 0xCB 0xCF
            SET(1, ref A);
        }
        private void SET_1_B()
        {
            // 0xCB 0xC8
            SET(1, ref B);
        }
        private void SET_1_C()
        {
            // 0xCB 0xC9
            SET(1, ref C);
        }
        private void SET_1_D()
        {
            // 0xCB 0xCA
            SET(1, ref D);
        }
        private void SET_1_E()
        {
            // 0xCB 0xCB
            SET(1, ref E);
        }
        private void SET_1_H()
        {
            // 0xCB 0xCC
            SET(1, ref H);
        }
        private void SET_1_L()
        {
            // 0xCB 0xCD
            SET(1, ref L);
        }
        private void SET_1__HL_()
        {
            // 0xCB 0xCE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 1))));
            InstructionCycles = 16;
        }

        private void SET_2_A()
        {
            // 0xCB 0xD7
            SET(2, ref A);
        }
        private void SET_2_B()
        {
            // 0xCB 0xD0
            SET(2, ref B);
        }
        private void SET_2_C()
        {
            // 0xCB 0xD1
            SET(2, ref C);
        }
        private void SET_2_D()
        {
            // 0xCB 0xD2
            SET(2, ref D);
        }
        private void SET_2_E()
        {
            // 0xCB 0xD3
            SET(2, ref E);
        }
        private void SET_2_H()
        {
            // 0xCB 0xD4
            SET(2, ref H);
        }
        private void SET_2_L()
        {
            // 0xCB 0xD5
            SET(2, ref L);
        }
        private void SET_2__HL_()
        {
            // 0xCB 0xD6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 2))));
            InstructionCycles = 16;
        }

        private void SET_3_A()
        {
            // 0xCB 0xDF
            SET(3, ref A);
        }
        private void SET_3_B()
        {
            // 0xCB 0xD8
            SET(3, ref B);
        }
        private void SET_3_C()
        {
            // 0xCB 0xD9
            SET(3, ref C);
        }
        private void SET_3_D()
        {
            // 0xCB 0xDA
            SET(3, ref D);
        }
        private void SET_3_E()
        {
            // 0xCB 0xDB
            SET(3, ref E);
        }
        private void SET_3_H()
        {
            // 0xCB 0xDC
            SET(3, ref H);
        }
        private void SET_3_L()
        {
            // 0xCB 0xDD
            SET(3, ref L);
        }
        private void SET_3__HL_()
        {
            // 0xCB 0xDE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 3))));
            InstructionCycles = 16;
        }

        private void SET_4_A()
        {
            // 0xCB 0xE7
            SET(4, ref A);
        }
        private void SET_4_B()
        {
            // 0xCB 0xE0
            SET(4, ref B);
        }
        private void SET_4_C()
        {
            // 0xCB 0xE1
            SET(4, ref C);
        }
        private void SET_4_D()
        {
            // 0xCB 0xE2
            SET(4, ref D);
        }
        private void SET_4_E()
        {
            // 0xCB 0xE3
            SET(4, ref E);
        }
        private void SET_4_H()
        {
            // 0xCB 0xE4
            SET(4, ref H);
        }
        private void SET_4_L()
        {
            // 0xCB 0xE5
            SET(4, ref L);
        }
        private void SET_4__HL_()
        {
            // 0xCB 0xE6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 4))));
            InstructionCycles = 16;
        }

        private void SET_5_A()
        {
            // 0xCB 0xEF
            SET(5, ref A);
        }
        private void SET_5_B()
        {
            // 0xCB 0xE8
            SET(5, ref B);
        }
        private void SET_5_C()
        {
            // 0xCB 0xE9
            SET(5, ref C);
        }
        private void SET_5_D()
        {
            // 0xCB 0xEA
            SET(5, ref D);
        }
        private void SET_5_E()
        {
            // 0xCB 0xEB
            SET(5, ref E);
        }
        private void SET_5_H()
        {
            // 0xCB 0xEC
            SET(5, ref H);
        }
        private void SET_5_L()
        {
            // 0xCB 0xED
            SET(5, ref L);
        }
        private void SET_5__HL_()
        {
            // 0xCB 0xEE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 5))));
            InstructionCycles = 16;
        }

        private void SET_6_A()
        {
            // 0xCB 0xF7
            SET(6, ref A);
        }
        private void SET_6_B()
        {
            // 0xCB 0xF0
            SET(6, ref B);
        }
        private void SET_6_C()
        {
            // 0xCB 0xF1
            SET(6, ref C);
        }
        private void SET_6_D()
        {
            // 0xCB 0xF2
            SET(6, ref D);
        }
        private void SET_6_E()
        {
            // 0xCB 0xF3
            SET(6, ref E);
        }
        private void SET_6_H()
        {
            // 0xCB 0xF4
            SET(6, ref H);
        }
        private void SET_6_L()
        {
            // 0xCB 0xF5
            SET(6, ref L);
        }
        private void SET_6__HL_()
        {
            // 0xCB 0xF6
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 6))));
            InstructionCycles = 16;
        }

        private void SET_7_A()
        {
            // 0xCB 0xFF
            SET(7, ref A);
        }
        private void SET_7_B()
        {
            // 0xCB 0xF8
            SET(7, ref B);
        }
        private void SET_7_C()
        {
            // 0xCB 0xF9
            SET(7, ref C);
        }
        private void SET_7_D()
        {
            // 0xCB 0xFA
            SET(7, ref D);
        }
        private void SET_7_E()
        {
            // 0xCB 0xFB
            SET(7, ref E);
        }
        private void SET_7_H()
        {
            // 0xCB 0xFC
            SET(7, ref H);
        }
        private void SET_7_L()
        {
            // 0xCB 0xFD
            SET(7, ref L);
        }
        private void SET_7__HL_()
        {
            // 0xCB 0xFE
            _mmu.Write(HL, (byte)(_mmu.Read(HL) | (byte)(Math.Pow(2, 7))));
            InstructionCycles = 16;
        }

        #endregion

        #region HELPER FUNCTIONS
        private void INC(ref Register r)
        {
            FlagN = 0;
            FlagH = flagH(r, 1);
            r++;
            FlagZ = flagZ(r);
            PC++;
            InstructionCycles = 4;
        }
        private void DEC(ref Register r)
        {
            FlagN = 1;
            FlagH = flagHSub(r, 1);
            r--;
            FlagZ = flagZ(r);
            PC++;
            InstructionCycles = 4;
        }
        private void ADD(byte r)
        {
            FlagN = 0;
            FlagH = flagH(A, r);
            FlagC = flagC(A + r);
            A += r;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }
        private void ADD_HL(ushort r)
        {
            FlagN = 0;
            FlagH = flagH(HL, r);
            FlagC = flagC((HL + r) >> 8);
            HL += r;
            PC++;
            InstructionCycles = 8;
        }
        private void ADC(byte r)
        {
            int result = A + r + FlagC;
            FlagZ = flagZ((byte)result);
            FlagN = 0;
            FlagH = flagHCarry(A, r); 
            FlagC = flagC(result);
            A = (byte)result;
            PC++;
            InstructionCycles = 4;
        }
        private void SUB(byte r)
        {
            FlagN = 1;
            FlagH = flagHSub(A, r);
            FlagC = flagC(A - r);
            A -= r;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }
        private void SBC(byte r)
        {
            int result = A - r - FlagC;
            FlagZ = flagZ((byte)result);
            FlagN = 1;
            FlagH = flagHSubCarry(A, r);
            FlagC = flagC(result);
            A = (byte)result;
            PC++;
            InstructionCycles = 4;
        }
        private void AND(byte r)
        {
            FlagN = 0;
            FlagH = 1;
            FlagC = 0;
            A &= r;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }
        private void OR(byte r)
        {
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A |= r;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }
        private void XOR(byte r)
        {
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            A ^= r;
            FlagZ = flagZ(A);
            PC++;
            InstructionCycles = 4;
        }
        private void CP(byte r)
        {
            FlagN = 1;
            FlagH = flagHSub(A, r);
            FlagC = flagC(A - r);
            FlagZ = flagZ(A - r);
            PC++;
            InstructionCycles = 4;
        }
        private void JR_r8_(bool condition)
        {
            InstructionCycles = 8;
            if (condition)
            {
                PC = (ushort)(PC + 2 + r8);
                InstructionCycles = 12;
            }
            else
            {
                PC += 2;
            }
        }
        private void JP_a16_(bool condition)
        {
            InstructionCycles = 12;
            if (condition)
            {
                PC = d16;
                InstructionCycles = 16;
            }
            else
            {
                PC += 3;
            }
        }
        private void CALL_a16_(bool condition)
        {
            InstructionCycles = 12;
            ushort address = d16;
            PC += 3;
            if (condition)
            {
                SP -= 2;
                _mmu.WriteWord(SP, PC);
                PC = address;
                InstructionCycles = 24;
            }
        }
        private void RET_(bool conditon)
        {
            InstructionCycles = 8;
            if (conditon)
            {
                PC = _mmu.ReadWord(SP);
                SP += 2;
                InstructionCycles = 20;
            }
            else
            {
                PC++;
            }
        }
        private void RST(ushort address)
        {
            PC++;
            SP -= 2;
            _mmu.WriteWord(SP, PC);
            PC = address;
            InstructionCycles = 16;
        }
        private void RLC(ref Register r)
        {
            // Rotate Left: 7 -> C, 7 -> 0
            FlagN = 0;
            FlagH = 0;
            FlagC = r[7];
            r = (byte)(((r << 1) | r[7]) & 0xFF);
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void RRC(ref Register r)
        {
            // Rotate Right: 0 -> C, 0 -> 7
            FlagN = 0;
            FlagH = 0;
            int newC = r[0];
            r = (byte)((r >> 1) | (r[0] << 7));
            FlagC = newC;
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void RL(ref Register r)
        {
            // Rotate Left: 7 -> C, C -> 0
            FlagN = 0;
            FlagH = 0;
            int newC = r[7];
            r = (byte)(((r << 1) | FlagC) & 0xFF);
            FlagC = newC;
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void RR(ref Register r)
        {
            // Rotate Right: 0 -> C, C -> 7
            FlagN = 0;
            FlagH = 0;
            int newC = r[0];
            r = (byte)((r >> 1) | (FlagC << 7));
            FlagC = newC;
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void SLA(ref Register r)
        {
            // Shift Left: 7 -> C, "0" -> 0
            FlagN = 0;
            FlagH = 0;
            FlagC = r[7];
            int temp = (r << 1);
            r = (byte)(temp & 0xFF);
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void SRA(ref Register r)
        {
            // Shift Right: 0 -> C, 7 -> 7
            FlagN = 0;
            FlagH = 0;
            FlagC = r[0];
            r = (byte)((r >> 1) | (r[7] << 7));
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void SWAP(ref Register r)
        {
            // Swap upper & lower nibbles
            FlagN = 0;
            FlagH = 0;
            FlagC = 0;
            r = (byte)(((r & 0x0F) << 4) + ((r & 0xF0) >> 4));
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void SRL(ref Register r)
        {
            // Shift Right: 0 -> C, "0" -> 7
            FlagN = 0;
            FlagH = 0;
            FlagC = r[0];
            r = (byte)(r >> 1);
            FlagZ = flagZ(r);
            InstructionCycles = 8;
        }
        private void BIT(int num, Register r)
        {
            // Test Bit
            InstructionCycles = 8;
            FlagN = 0;
            FlagH = 1;
            FlagZ = flagZ(r[num]);
        }
        private void RES(int num, ref Register r)
        {
            // Reset Bit
            r[num] = 0;
            InstructionCycles = 8;
        }
        private void SET(int num, ref Register r)
        {
            // Set Bit
            r[num] = 1;
            InstructionCycles = 8;
        }
        #endregion

        #region FLAG FUNCTIONS
        // with help of https://github.com/BluestormDNA/ProjectDMG/blob/master/ProjectDMG/DMG/CPU.cs

        // Z = Zero Flag
        private int flagZ(int value)
        {
            if (value == 0) { return 1; } else { return 0; }
        }
        // H = Half-Carry Flag
        private int flagH(byte a, byte b)
        {
            // mask to 4 bits then sum up and return 5th bit for flag
            return (((a & 0xF) + (b & 0xF)) >> 4) & 1;
        }
        private int flagH(ushort a, ushort b)
        {
            // special version for carry on bit 11
            return (((a & 0xFFF) + (b & 0xFFF)) >> 12) & 1;
        }
        private int flagHCarry(byte a, byte b)
        {
            return (((a & 0xF) + (b & 0xF) + FlagC) >> 4) & 1;
        }
        private int flagHSub(byte a, byte b)
        {
            // mask to 4 bits; if second number bigger than first number (negative result) there is a carry on 5th bit
            if ((a & 0xF) < (b & 0xF)) { return 1; } else { return 0; }
        }
        private int flagHSubCarry(byte a, byte b)
        {
            if ((a & 0xF) < ((b & 0xF) + FlagC)) { return 1; } else { return 0; }
        }

        // C = Carry Flag
        private int flagC(int sum)
        {
            // return 9th bit
            return (sum >> 8) & 1;
        }

        #endregion

        #endregion

    }
}
