﻿using System;
using System.IO;
using System.Linq;

namespace PPTTools {
    namespace Modules {
        public class KPP: Module {
            private static readonly string ModuleIdentifier = "kpp";

            private int[] keyStates = new int[7] {0, 0, 0, 0, 0, 0, 0}, queue = new int[5];
            private int keystrokes, state, pieces, piece;
            private bool register = false, door = false;

            public delegate void KPPEventHandler(Decimal KPP);
            public event KPPEventHandler Changed;

            private void Raise() {
                if (Changed != null) {
                    if (pieces == 0) {
                        Changed.Invoke(0);
                    } else {
                        Changed.Invoke(Decimal.Divide(keystrokes, pieces));
                    }
                }
            }

            public void Reset() {
                keystrokes = pieces = 0;
                queue = new int[5];
                piece = 255;
                register = door = false;

                Raise();
            }

            public void Update() {
                int drop = GameHelper.PieceDropped(GameHelper.GameState.playerIndex);

                if (drop != state) {
                    if (drop == 1) {
                        pieces++;
                        register = true;
                        door = false;
                        
                        if (keyStates[2] == 0) {
                            keystrokes++;
                        }
                    }

                    state = drop;
                }

                int current = GameHelper.CurrentPiece(GameHelper.GameState.playerIndex);
                int piecesAddress = GameHelper.NextPointer(GameHelper.GameState.playerIndex);
                int[] cQueue = new int[5];
                for (int i = 0; i < 5; i++) {
                    cQueue[i] = GameHelper.Game.ReadByte(new IntPtr(piecesAddress + i * 0x04));
                }

                if ((register && !cQueue.SequenceEqual(queue) && current == queue[0]) || (current != piece && piece == 255)) {
                    door = true;
                    register = false;
                }

                if (door) {
                    for (int i = 0; i < 7; i++) {
                        int key = GameHelper.Keystroke(i);

                        if (key != keyStates[i]) {
                            if (key == 1 && GameHelper.BigFrames() >= 147)
                                keystrokes++;

                            keyStates[i] = key;
                        }
                    }
                }

                piece = current;

                if (!register)
                    queue = (int[])cQueue.Clone();

                Raise();
            }

            public KPP(): base(ModuleIdentifier) {
                Reset();
                Changed += Write;
            }

            private void Write(Decimal kpp) {
                if (File.Exists(filename)) {
                    StreamWriter sw = new StreamWriter(filename);
                    sw.WriteLine(kpp.ToString("0.000 KPP"));
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}