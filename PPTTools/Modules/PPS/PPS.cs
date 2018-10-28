﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PPTTools {
    namespace Modules {
        public class PPS {
            private int pieces, state, frames;

            public delegate void PPSEventHandler(Decimal PPS);
            public event PPSEventHandler Changed;

            private void Raise() {
                if (Changed != null) {
                    if (frames == 0) {
                        Changed.Invoke(0);
                    } else {
                        Changed.Invoke(Decimal.Divide(pieces, frames) * 60);
                    }
                }
            }

            public void Reset() {
                pieces = state = 0;

                Raise();
            }

            public void Update() {
                frames = GameHelper.SmallFrames();
                int drop = GameHelper.PieceDropped(GameHelper.GameState.playerIndex);

                if (drop != state) {
                    if (drop == 1)
                        pieces++;

                    state = drop;
                }

                Raise();
            }

            public PPS() {
                Reset();
            }
        }
    }
}