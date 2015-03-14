using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace ProgCom
{
    public class Monitor : PartModule, IPCHardware, PCGUIListener
    {
        Texture2D image;
        protected Rect windowPos;
        Int32[] mem;
        UInt16 pointer;
        UInt16 charSetPtr;
        UInt16 colorPointer;
        UInt16 modePtr;
        UInt16 scrollPointer;
        Color32[] colors;
        Color32[] imageBuffer;
        protected int windowID;
        UInt16 address = 60910;
        GUIStates guis = new GUIStates();
        bool updateScreen = false;
        bool updateCols = false;


        private void clearScreen(Color32 c)
        {
            for (int x = 0; x < 256*256; x++) {
                imageBuffer[x] = c;
            }
        }

        public override void  OnStart(PartModule.StartState state) {
 	        base.OnStart(state);
            if (state.Equals(PartModule.StartState.Editor)) return;//don't start stuff in the editor
            windowID = Util.random();
            mem = new Int32[4626];
            pointer = 530;
            charSetPtr = 18;
            colorPointer = 2;
            modePtr = 1;
            scrollPointer = 0;
            colors = new Color32[16];
            for (int i = 0; i < 16; ++i) {
                colors[i] = new Color32();
                colors[i].a = 255;
            }

            imageBuffer = new Color32[256*256];
            image = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            image.filterMode = FilterMode.Point;
            windowPos = new Rect();
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 100, 100);
            }
            //Set all the pixels to black. If you don't do this the image contains random junk.
            for (int y = 0; y < image.height; y++) {
                for (int x = 0; x < image.width; x++) {
                    image.SetPixel(x, y, Color.black);
                }
            }
            image.Apply();

            //init monitor drawing
            RenderingManager.AddToPostDrawQueue(3, new Callback(draw));

            //initialise fonts and colors and things
            //init default values in memory
            int index = charSetPtr;
            foreach (UInt32 font in getDefaultFont()) {
                mem[index] = (Int32)font;
                ++index;
            }
            index = colorPointer;
            foreach (Int32 col in getDefaultColors()) {
                mem[index] = col;
                ++index;
            }
            updateColors();
        }

        private void updateColors()
        {
            for (int i = 0; i < 16; ++i) {
                int colData = mem[colorPointer + i];
                colors[i].b = (byte)colData;
                colors[i].g = (byte)(colData >> 8);
                colors[i].r = (byte)(colData >> 16);
            }
        }

        


        //private float dTime = 0.0f;
        //private const float redrawTime = 1.0f / 30.0f;


        int drawIndex = 0;
        void putPixel(int color)
        {
            if (drawIndex >= (256 * 256)) {
                return;
            }

            //what bits do we have?

            //index:    name:   what:
            //0-1       R       Resolution
            //2-3       --      unused
            //4-5       CDE      Color Depth Expected


            Color32 displayColor = colors[0];
            int colorDepth = (mem[modePtr] >> 20) & 3;
            switch (colorDepth) {
                case 0:
                    displayColor = colors[color & 15];
                    break;
                case 1:
                    displayColor = colors[color & 1];
                    break;
                case 2:
                    displayColor = new Color32();
                    displayColor.a = 255;
                    displayColor.r = (byte)color;
                    displayColor.g = (byte)color;
                    displayColor.b = (byte)color;
                    break;
                case 3:
                    displayColor = new Color32();
                    displayColor.a = 255;
                    displayColor.r = (byte)(((color & 65535) >> 7) & 248);
                    displayColor.g = (byte)(((color & 65535) >> 2) & 248);
                    displayColor.b = (byte)(((color & 65535) << 3) & 248);
                    break;
            }

            //determine the mode of the pixel-putting
            int pixelSize = ((mem[modePtr]>>16) & 3);
            switch (pixelSize) {
                case 0:
                    imageBuffer[(drawIndex)^0xFF00] = displayColor;
                    drawIndex++;
                    break;
                case 1:
                    imageBuffer[(drawIndex)^0xFF00] = displayColor;
                    imageBuffer[(drawIndex+1)^0xFF00] = displayColor;
                    drawIndex += 2;
                    break;
                case 2:
                    imageBuffer[(drawIndex)^0xFF00] = displayColor;
                    imageBuffer[(drawIndex+1)^0xFF00] = displayColor;
                    imageBuffer[(drawIndex+256)^0xFF00] = displayColor;
                    imageBuffer[(drawIndex+256+1)^0xFF00] = displayColor;
                    drawIndex += 2;
                    drawIndex += (drawIndex & 256);//make sure we skip the next line if we are at the end of our current line
                    break;
                case 3:
                    for (int y = 0; y < 4; y++) {
                        for (int x = 0; x < 4; x++) {
                            imageBuffer[(drawIndex + y*256 + x)^0xFF00] = displayColor;
                        }
                    }
                    drawIndex += 4;
                    drawIndex += (drawIndex & 256);
                    drawIndex += (drawIndex & 512);//make sure we skip three lines if we are at the end of our current line
                    break;
            }
        }

        void streamPixels()
        {
            //what bits do we have?
            //index:    name:   what:
            //0         BTB     bitmap toggle bit
            //1         HCTB    High-color toggle bit
            //2         EM      enable monochrome
            //3         ICDB    increased color depth bitmaps
            //4         DDS     double data set
            //5         DT      double transmission, sends each pixel twice


            //what modes do we have?

            //0: tile modes:
                //tiled 2x 24-bit color, 16 color palette, for 256*256, STANDARD
                    //already implemented and documented <- done
                //tiled 4x 24-bit color, 16 color palette, for 256*128
                    //unlocked by enabling HCTB <- done
                //tiled 8-bit color, b&w versions of above
                    //unlocked by toggling EM
                //tiled 2x 15-bit color, 1 bit bitmap, for 256*128
                    //unlocked by toggling HCTB & DDS
            //1: bitmapped modes:
                //bitmapped  1 bit, STANDARD <- done
                    //already implemented and documented
                //bitmapped  4 bit, up to 256*128 <- done
                    //unlock by enable BTB & HCTB + CDE4
                //bitmapped  8 bit, up to 128*128 w. DDS <- done
                    //unlock by enabling BTB & HCTB & FM + CDE8
                //bitmapped 15 bit, 64*64 <- done
                    //unlock by enabling BTB & HCTB & ICDB + CDE15

            //TODO:
            //1: implement additional blocky modes
            //2: implement monochrome enable bit
            //3: implement scrolling properly

            //reset pixel streamer
            drawIndex = 0;
            //clear Screen
            clearScreen(new Color32(0,0,0,255));

            bool doubleTrans = ((mem[modePtr] & 32) == 32);

            if ((mem[modePtr] & 1) == 0) {
                //tiled mode
                int dataSet = 512;
                if ((mem[modePtr] & 16) == 16) dataSet *= 2;
                int finalMask = (mem[modePtr]&4) == 4 ? 1 : 7;
                bool ColExtendMode = ((mem[modePtr] & 2) == 2);
                Int32[] cache = new Int32[64];
                for (int x = 0; x < dataSet; x += 16) {
                    //fetch all tiles
                    for (int i = 0; i < 64; i+=4) {
                        Int32 currentChar = mem[pointer + ((mem[scrollPointer]+(i >> 2) + x)&4095)];
                        cache[i]    = mem[charSetPtr + (currentChar & 0xff )*2  ];
                        cache[i+1]  = mem[charSetPtr + (currentChar & 0xff )*2+1];
                        cache[i+2]  = mem[charSetPtr + ((currentChar >> 16) & 0xff )*2  ];
                        cache[i+3]  = mem[charSetPtr + ((currentChar >> 16) & 0xff )*2+1];
                    }
                    //draw tiles line-by-line
                    for (int h = 0; h < 8; h++) {
                        for (int w = 0; w < 256; w++) {
                            int maskShift;
                            int selectedBits;
                            int colorTableResult;
                            if(ColExtendMode) {
                                //get 2 bits of data to index 4 2 bit sections in the on-char color index. Append color index as higher two bits in number
                                selectedBits = cache[(w / 8) * 2 + (h / 4)];
                                maskShift = ((w & 7) & 6) + (h & 3) * 8;
                                selectedBits = selectedBits & (3 << (6^maskShift));
                                selectedBits = selectedBits >> (6^maskShift);
                                
                                //find the index in the color lookup table on the character we are currently rendering according to the 2 bit index in the character symbol data
                                colorTableResult = mem[pointer + (mem[scrollPointer]+ x + (w / 16))&4095] & (3 << (8+selectedBits * 2 + (w&8)*2));
                                colorTableResult = colorTableResult >> (8+selectedBits * 2 + (w & 8) * 2);
                                
                                //mix colorTableResult with the bit mode to enable us to use every color in the table
                                colorTableResult = (colorTableResult << 2) | selectedBits;

                                //render pixel
                                putPixel(colorTableResult&finalMask);
                                if (doubleTrans) putPixel(colorTableResult & finalMask);
                                w++;//make sure that the resolution is properly halved
                            } else {
                                //get 1 bit of data to index 2 4 bit sections in the on-char color index.
                                selectedBits = cache[(w / 8) * 2 + h / 4];
                                maskShift = (w & 7) + (h & 3) * 8;
                                selectedBits = selectedBits & (1 << (7^maskShift));//note that the top bit of each byte is to be read first
                                selectedBits = selectedBits >> (7^maskShift);
                                //find the 4-bit index in the color lookup table on the character we are currently rendering according to the 1 bit index in the character symbol data
                                colorTableResult = mem[pointer + (mem[scrollPointer]+x + (w / 16))&4095] & (7 << (8+(4^selectedBits * 4) + (w & 8) * 2));
                                colorTableResult = colorTableResult >> (8+(4^selectedBits * 4) + (w & 8) * 2);

                                //render pixel
                                putPixel(colorTableResult&finalMask);
                                if (doubleTrans) putPixel(colorTableResult & finalMask);
                            }
                        }
                    }
                }
            } else {
                //bitmap mode
                int bitsPerPixel = 1;
                if ((mem[modePtr] & 2) == 2) {
                    bitsPerPixel = 4;
                }
                if ((mem[modePtr] & 4) == 4) {
                    bitsPerPixel = 8;
                }
                if ((mem[modePtr] & 8) == 8) {
                    if (bitsPerPixel == 4) {
                        bitsPerPixel = 16;
                    }
                    bitsPerPixel *= 2;
                }
                int dataSetSize = 2048;
                if ((mem[modePtr] & 16) == 16) {
                    dataSetSize *= 2;
                }
                int bitMask = 0xffff;
                if ((mem[modePtr] & 4) == 4) {
                    bitMask = 0xff;
                }
                //loop through the data set and send all data to screen
                for (int x = 0; x < dataSetSize; x++) {
                    int data = mem[pointer + ((mem[scrollPointer]+x) & 4095)];
                    for (int b = 0; b < 32; b += bitsPerPixel) {
                        putPixel( ((data >> b) & (1<<b)-1) & bitMask );
                        if (doubleTrans) putPixel(((data >> b) & (1 << b) - 1) & bitMask);
                    }
                }
            }

        }


        void drawImage()
        {
            //lock screen to 30fps
            //dTime += UnityEngine.Time.deltaTime;
            //if (dTime < redrawTime) return;
            //while (dTime >= redrawTime) {
            //    dTime -= redrawTime;//this step is in a while loop to adjust for slow refresh rates (ie game runs at less than 30fps)
            //}

            if (!updateScreen) {
                return;
            } else {
                updateScreen = false;
            }
            if (updateCols) {
                updateColors();
                updateCols = false;
            }
            if (mem[modePtr] == 0) {
                //colour character set
                for (int y = 0; y < 32; ++y) {
                    int offset = (31-y) * 2048;
                    for (int x = 0; x < 16; ++x) {
                        //read four characters and all colours from text buffer
                        //since each character+colour is 16 bit, this should net us 2 characters at a time
                        Int32 AB = mem[pointer + ((y * 16 + x + mem[scrollPointer]) & 4095)];
                        UInt16 A = (UInt16)(AB & 0xffff);
                        UInt16 B = (UInt16)((AB >> 16) & 0xffff);
                        //load the font for all characters
                        //each character has 2 words each, so we need to read 4 total
                        Int32 Ax = mem[charSetPtr + (A & 0xff) * 2];
                        Int32 Ay = mem[charSetPtr + (A & 0xff) * 2 + 1];
                        Int32 Bx = mem[charSetPtr + (B & 0xff) * 2];
                        Int32 By = mem[charSetPtr + (B & 0xff) * 2 + 1];
                        //render each character
                        UInt64 fontMap = ((UInt64)(UInt32)Ay) << 32 | ((UInt64)(UInt32)Ax);
                        fontDraw(colors[(A >> 8) & 0xf], colors[(A >> 12) & 0xf], fontMap, offset);
                        fontMap = ((UInt64)(UInt32)By) << 32 | ((UInt64)(UInt32)Bx);
                        fontDraw(colors[(B >> 8) & 0xf], colors[(B >> 12) & 0xf], fontMap, offset+8);
                        offset += 16;
                    }
                }
            } else if (mem[modePtr] == 1) {
                //monochrome display
                for (int y = 0; y < 256; ++y) {
                    int offset = ((255 - y) * 256);
                    for (int x = 0; x < 8; ++x) {
                        Int32 pixels = mem[pointer + ((y * 8 + x + mem[2047]) & 2047)];
                        for (int i = 0; i < 32; ++i) {
                            bool set = ((pixels >> i) & 1) != 0;
                            imageBuffer[offset + i] = set ? colors[1] : colors[0];
                        }
                        offset += 32;
                    }
                }
            } else {
                streamPixels();
            }
            image.SetPixels32(imageBuffer);
            image.Apply(false);
        }

        //draws a single character to the screen
        //colA and colB denotes colour, charBits is a monochrome 8x8 tile flattened into bits of an integer, xpos, ypos are screen destination upper left
        private void fontDraw(Color32 colA, Color32 colB, UInt64 charBits, int pointer)
        {
            pointer += 256 * 7;
            //move the color data into the texture buffer
            for (int i = 0; i < 64; i+=8) {
                int currentBits = (int)(charBits >> i) & 0xff;
                for (int j = 0; j < 8; ++j) {
                    bool bit = ((currentBits << j) & 0x80) != 0;
                    imageBuffer[pointer + j] = bit ? colA : colB;
                }
                pointer -= 256;
            }
        }

        int guiScaleFactor = 1;
        //Now to display this we can include it in a GUI window. See the "Creating a Window GUI" example for how to bring up a window.
        //In the window GUI code we just need this line to put our Texture2D on the screen:
        protected void windowGUI(int windowID)
        {
            //normal gui style
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            //gui style when off
            GUIStyle offSty = new GUIStyle(GUI.skin.button);
            offSty.normal.textColor = offSty.focused.textColor = Color.grey;
            offSty.hover.textColor = offSty.active.textColor = Color.grey;
            offSty.onNormal.textColor = offSty.onFocused.textColor = offSty.onHover.textColor = offSty.onActive.textColor = Color.green;
            offSty.padding = new RectOffset(8, 8, 8, 8);
            
            GUIStyle imageStyle = new GUIStyle();
            imageStyle.stretchHeight = true;
            imageStyle.stretchWidth = true;
            imageStyle.fixedHeight = 0;
            imageStyle.fixedWidth = 0;

            GUILayout.BeginVertical();
            GUILayout.Box("",GUILayout.Width((float)256*guiScaleFactor), GUILayout.Height((float)256*guiScaleFactor));
            GUI.DrawTexture(new Rect(7, 28, 256*guiScaleFactor, 256*guiScaleFactor), image, ScaleMode.StretchToFill);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("1x", guiScaleFactor == 1 ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
            {
                guiScaleFactor = 1;
                windowPos.width = 0;
                windowPos.height = 0;
            }
            if (GUILayout.Button("2x", guiScaleFactor == 2 ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
            {
                guiScaleFactor = 2;
                windowPos.width = 0;
                windowPos.height = 0;
            }
            if (GUILayout.Button("3x", guiScaleFactor == 3 ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
            {
                guiScaleFactor = 3;
                windowPos.width = 0;
                windowPos.height = 0;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void draw()
        {
            if (guis.mon) {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(windowID, windowPos, windowGUI, "Monitor", GUILayout.MinWidth(100));
            }
        }

        public override void OnUpdate()
        {
            if (guis.mon)
                drawImage();
        }

        private UInt32[] getDefaultFont()
        {
            UInt32[] cga_8 = {
            0x0, 0x0,
            0x81a5817e, 0x7e8199bd,
            0xffdbff7e, 0x7effe7c3,
            0xfefefe6c, 0x10387c,
            0xfe7c3810, 0x10387c,
            0xfe387c38, 0x3810d6fe,
            0x7c381010, 0x38107cfe,
            0x3c180000, 0x183c,
            0xc3e7ffff, 0xffffe7c3,
            0x42663c00, 0x3c6642,
            0xbd99c3ff, 0xffc399bd,
            0x7d0f070f, 0x78cccccc,
            0x6666663c, 0x187e183c,
            0x303f333f, 0xe0f07030,
            0x637f637f, 0xc0e66763,
            0xe73cdb18, 0x18db3ce7,
            0xfef8e080, 0x80e0f8,
            0xfe3e0e02, 0x20e3e,
            0x187e3c18, 0x183c7e18,
            0x66666666, 0x660066,
            0x7bdbdb7f, 0x1b1b1b,
            0x6c38633e, 0x78cc386c,
            0x0, 0x7e7e7e,
            0x187e3c18, 0xff183c7e,
            0x187e3c18, 0x181818,
            0x18181818, 0x183c7e,
            0xfe0c1800, 0x180c,
            0xfe603000, 0x3060,
            0xc0c00000, 0xfec0,
            0xff662400, 0x2466,
            0x7e3c1800, 0xffff,
            0x7effff00, 0x183c,
            0x0, 0x0,
            0x30787830, 0x300030,
            0x6c6c6c, 0x0,
            0x6cfe6c6c, 0x6c6cfe,
            0x78c07c30, 0x30f80c,
            0x18ccc600, 0xc66630,
            0x76386c38, 0x76ccdc,
            0xc06060, 0x0,
            0x60603018, 0x183060,
            0x18183060, 0x603018,
            0xff3c6600, 0x663c,
            0xfc303000, 0x3030,
            0x0, 0x60303000,
            0xfc000000, 0x0,
            0x0, 0x303000,
            0x30180c06, 0x80c060,
            0xdecec67c, 0x7ce6f6,
            0x30307030, 0xfc3030,
            0x380ccc78, 0xfccc60,
            0x380ccc78, 0x78cc0c,
            0xcc6c3c1c, 0x1e0cfe,
            0xcf8c0fc, 0x78cc0c,
            0xf8c06038, 0x78cccc,
            0x180cccfc, 0x303030,
            0x78cccc78, 0x78cccc,
            0x7ccccc78, 0x70180c,
            0x303000, 0x303000,
            0x303000, 0x60303000,
            0xc0603018, 0x183060,
            0xfc0000, 0xfc00,
            0xc183060, 0x603018,
            0x180ccc78, 0x300030,
            0xdedec67c, 0x78c0de,
            0xcccc7830, 0xccccfc,
            0x7c6666fc, 0xfc6666,
            0xc0c0663c, 0x3c66c0,
            0x66666cf8, 0xf86c66,
            0x786862fe, 0xfe6268,
            0x786862fe, 0xf06068,
            0xc0c0663c, 0x3e66ce,
            0xfccccccc, 0xcccccc,
            0x30303078, 0x783030,
            0xc0c0c1e, 0x78cccc,
            0x786c66e6, 0xe6666c,
            0x606060f0, 0xfe6662,
            0xfefeeec6, 0xc6c6d6,
            0xdef6e6c6, 0xc6c6ce,
            0xc6c66c38, 0x386cc6,
            0x7c6666fc, 0xf06060,
            0xcccccc78, 0x1c78dc,
            0x7c6666fc, 0xe6666c,
            0x3060cc78, 0x78cc18,
            0x3030b4fc, 0x783030,
            0xcccccccc, 0xfccccc,
            0xcccccccc, 0x3078cc,
            0xd6c6c6c6, 0xc6eefe,
            0x386cc6c6, 0xc66c38,
            0x78cccccc, 0x783030,
            0x188cc6fe, 0xfe6632,
            0x60606078, 0x786060,
            0x183060c0, 0x2060c,
            0x18181878, 0x781818,
            0xc66c3810, 0x0,
            0x0, 0xff000000,
            0x183030, 0x0,
            0xc780000, 0x76cc7c,
            0x7c6060e0, 0xdc6666,
            0xcc780000, 0x78ccc0,
            0x7c0c0c1c, 0x76cccc,
            0xcc780000, 0x78c0fc,
            0xf0606c38, 0xf06060,
            0xcc760000, 0xf80c7ccc,
            0x766c60e0, 0xe66666,
            0x30700030, 0x783030,
            0xc0c000c, 0x78cccc0c,
            0x6c6660e0, 0xe66c78,
            0x30303070, 0x783030,
            0xfecc0000, 0xc6d6fe,
            0xccf80000, 0xcccccc,
            0xcc780000, 0x78cccc,
            0x66dc0000, 0xf0607c66,
            0xcc760000, 0x1e0c7ccc,
            0x76dc0000, 0xf06066,
            0xc07c0000, 0xf80c78,
            0x307c3010, 0x183430,
            0xcccc0000, 0x76cccc,
            0xcccc0000, 0x3078cc,
            0xd6c60000, 0x6cfefe,
            0x6cc60000, 0xc66c38,
            0xcccc0000, 0xf80c7ccc,
            0x98fc0000, 0xfc6430,
            0xe030301c, 0x1c3030,
            0x181818, 0x181818,
            0x1c3030e0, 0xe03030,
            0xdc76, 0x0,
            0x6c381000, 0xfec6c6,
            0xccc0cc78, 0x780c1878,
            0xcc00cc00, 0x7ecccc,
            0xcc78001c, 0x78c0fc,
            0x63cc37e, 0x3f663e,
            0xc7800cc, 0x7ecc7c,
            0xc7800e0, 0x7ecc7c,
            0xc783030, 0x7ecc7c,
            0xc0780000, 0x380c78c0,
            0x663cc37e, 0x3c607e,
            0xcc7800cc, 0x78c0fc,
            0xcc7800e0, 0x78c0fc,
            0x307000cc, 0x783030,
            0x1838c67c, 0x3c1818,
            0x307000e0, 0x783030,
            0xc66c38c6, 0xc6c6fe,
            0x78003030, 0xccfccc,
            0x60fc001c, 0xfc6078,
            0xc7f0000, 0x7fcc7f,
            0xfecc6c3e, 0xcecccc,
            0x7800cc78, 0x78cccc,
            0x7800cc00, 0x78cccc,
            0x7800e000, 0x78cccc,
            0xcc00cc78, 0x7ecccc,
            0xcc00e000, 0x7ecccc,
            0xcc00cc00, 0xf80c7ccc,
            0x663c18c3, 0x183c66,
            0xcccc00cc, 0x78cccc,
            0xc07e1818, 0x18187ec0,
            0xf0646c38, 0xfce660,
            0xfc78cccc, 0x3030fc30,
            0xfaccccf8, 0xc7c6cfc6,
            0x3c181b0e, 0x70d81818,
            0xc78001c, 0x7ecc7c,
            0x30700038, 0x783030,
            0x78001c00, 0x78cccc,
            0xcc001c00, 0x7ecccc,
            0xf800f800, 0xcccccc,
            0xeccc00fc, 0xccdcfc,
            0x3e6c6c3c, 0x7e00,
            0x386c6c38, 0x7c00,
            0x60300030, 0x78ccc0,
            0xfc000000, 0xc0c0,
            0xfc000000, 0xc0c,
            0xdeccc6c3, 0xfcc6633,
            0xdbccc6c3, 0x3cf6f37,
            0x18001818, 0x181818,
            0xcc663300, 0x3366,
            0x3366cc00, 0xcc66,
            0x88228822, 0x88228822,
            0xaa55aa55, 0xaa55aa55,
            0xeedb77db, 0xeedb77db,
            0x18181818, 0x18181818,
            0x18181818, 0x181818f8,
            0x18f81818, 0x181818f8,
            0x36363636, 0x363636f6,
            0x0, 0x363636fe,
            0x18f80000, 0x181818f8,
            0x6f63636, 0x363636f6,
            0x36363636, 0x36363636,
            0x6fe0000, 0x363636f6,
            0x6f63636, 0xfe,
            0x36363636, 0xfe,
            0x18f81818, 0xf8,
            0x0, 0x181818f8,
            0x18181818, 0x1f,
            0x18181818, 0xff,
            0x0, 0x181818ff,
            0x18181818, 0x1818181f,
            0x0, 0xff,
            0x18181818, 0x181818ff,
            0x181f1818, 0x1818181f,
            0x36363636, 0x36363637,
            0x30373636, 0x3f,
            0x303f0000, 0x36363637,
            0xf73636, 0xff,
            0xff0000, 0x363636f7,
            0x30373636, 0x36363637,
            0xff0000, 0xff,
            0xf73636, 0x363636f7,
            0xff1818, 0xff,
            0x36363636, 0xff,
            0xff0000, 0x181818ff,
            0x0, 0x363636ff,
            0x36363636, 0x3f,
            0x181f1818, 0x1f,
            0x181f0000, 0x1818181f,
            0x0, 0x3636363f,
            0x36363636, 0x363636ff,
            0x18ff1818, 0x181818ff,
            0x18181818, 0xf8,
            0x0, 0x1818181f,
            0xffffffff, 0xffffffff,
            0x0, 0xffffffff,
            0xf0f0f0f0, 0xf0f0f0f0,
            0xf0f0f0f, 0xf0f0f0f,
            0xffffffff, 0x0,
            0xdc760000, 0x76dcc8,
            0xf8cc7800, 0xc0c0f8cc,
            0xc0ccfc00, 0xc0c0c0,
            0x6c6cfe00, 0x6c6c6c,
            0x3060ccfc, 0xfccc60,
            0xd87e0000, 0x70d8d8,
            0x66666600, 0xc0607c66,
            0x18dc7600, 0x181818,
            0xcc7830fc, 0xfc3078cc,
            0xfec66c38, 0x386cc6,
            0xc6c66c38, 0xee6c6c,
            0x7c18301c, 0x78cccc,
            0xdb7e0000, 0x7edb,
            0xdb7e0c06, 0xc0607edb,
            0xf8c06038, 0x3860c0,
            0xcccccc78, 0xcccccc,
            0xfc00fc00, 0xfc00,
            0x30fc3030, 0xfc0030,
            0x30183060, 0xfc0060,
            0x30603018, 0xfc0018,
            0x181b1b0e, 0x18181818,
            0x18181818, 0x70d8d818,
            0xfc003030, 0x303000,
            0xdc7600, 0xdc76,
            0x386c6c38, 0x0,
            0x18000000, 0x18,
            0x0, 0x18,
            0xc0c0c0f, 0x1c3c6cec,
            0x6c6c6c78, 0x6c,
            0x60301870, 0x78,
            0x3c3c0000, 0x3c3c,
            0x0, 0x0
            };
            return cga_8;
        }

        private Int32[] getDefaultColors()
        {
            Int32[] c64Cols = {
            0x00000000,
	        0x00ffffff,
	        0x0068372b,
	        0x0070a4b2,
	        0x006f3d86,
	        0x00588d43,
	        0x00352879,
	        0x00b8c76f,
	        0x006f4f25,
	        0x00433900,
	        0x009a6759,
	        0x00444444,
	        0x006c6c6c,
	        0x009ad284,
	        0x006c5eb5,
	        0x00959595
            };
            return c64Cols;
        }

        public void connect()
        {
            
        }

        public void disconnect()
        {
            
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(address, 4626);
        }

        public int getSegmentCount()
        {
            return 1;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            
        }

        public int memRead(ushort position)
        {
            return mem[position - address];
        }

        public void memWrite(ushort position, int value)
        {
            updateScreen = true;
            position -= address;

            if (position > modePtr && position < charSetPtr) {
                updateCols = true;
            }

            mem[position] = value;
        }

        public void tick(int ticks)
        {
            
        }

        public void recGUIState(GUIStates g)
        {
            guis = g;
        }
    }
}