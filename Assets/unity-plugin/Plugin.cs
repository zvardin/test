using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animate
{
    public class LayerStateObject
    {
        public GameObject gobj;
        public Animation anim;
        public string LayerStateName;
        public string spriteName;
        public Dictionary<int, decomposedMat> arr = new Dictionary<int, decomposedMat>();
        public string type;
        public float zDepth;
        public LayerStateObject parent;
        public List<string> children = new List<string>();
        public Dictionary<int,int> sortingorder = new Dictionary<int, int>();
        public Dictionary<int, Color32> color = new Dictionary<int, Color32>();
        public Dictionary<int, bool> SpriteEnabled = new Dictionary<int, bool>();
    }

    [System.Serializable]
    public struct ParsedSpriteMap
    {
        public Atlas ATLAS;
        public meta meta;
    }

    [System.Serializable]
    public struct Atlas
    {
        public Sprite1[] SPRITES;
    }

    [System.Serializable]
    public struct Sprite1
    {
        public SubSprite SPRITE;
    }

    [System.Serializable]
    public struct SubSprite
    {
        public string name;
        public int x;
        public int y;
        public int w;
        public int h;
        public bool rotated;
    }
    public struct SpriteObject
    {
        public Sprite sp;
        public string name;
        public bool rotated; 
    }

    [System.Serializable]
    public struct ParsedAnimation
    {
        public AnimationFile ANIMATION;
        public SymbolDict SYMBOL_DICTIONARY;
        public meta metadata;
    }

    [System.Serializable]
    public struct AnimationFile
    {
        public string name;
        public StageProperties StageInstance;
        public string SYMBOL_name;
        public TimeLine TIMELINE;
    }

    [System.Serializable]
    public struct StageProperties
    {
        public InstanceProperties SYMBOL_Instance;
    }

    [System.Serializable]
    public class InstanceProperties
    {
        public string SYMBOL_name;
        public  string Instance_Name;
        public AtlasSprite bitmap;
        public string symbolType;
        public int firstFrame;
        public string loop;
        public Vector2 transformationPoint;
        public Matrix3 Matrix3D;
        public decomposedMat DecomposedMatrix;
        public Colour color;
    }

    [System.Serializable]
    public struct Matrix3
    {
    }
    [System.Serializable]
    public struct SymbolDict
    {
        public Symbol[] Symbols;
    }
    [System.Serializable]
    public struct meta
    {
        public float framerate;
        public Size size;
    }

    [System.Serializable]
    public class TimeLine
    {
        public Layer[] LAYERS;
        public int TimelineDuration;
    }

    [System.Serializable]
    public class Layer
    {
        public string Layer_name;
        public Frame[] Frames;
        public int LayerDuration;
    }

    [System.Serializable]
    public struct Frame
    {
        public string name;
        public int index;
        public int duration;
        public float zDepth;
        public Element[] elements;
    }

    [System.Serializable]
    public struct Element
    {
        public InstanceProperties SYMBOL_Instance;
        public AtlasSprite ATLAS_SPRITE_instance;
    }

    [System.Serializable]
    public struct AtlasSprite
    {
        public string name;
        public Vector3 Position;
    }

    [System.Serializable]
    public struct Symbol
    {
        public string SYMBOL_name;
        public TimeLine TIMELINE;
    }

    [System.Serializable]
    public struct decomposedMat
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scaling;
    }

    [System.Serializable]
    public struct Size
    {
        public float w;
        public float h;
    }

    [System.Serializable]
    public struct Colour
    {
        public string mode;
        public string tintColor;
        public float tintMultiplier;
        public float RedMultiplier;
        public float greenMultiplier;
        public float blueMultiplier;
        public float alphaMultiplier;
        public int redOffset;
        public int greenOffset;
        public int blueOffset;
        public int AlphaOffset;
        public float brightness;
     }

    public class MainAnimation
    {
        public Sprite[] sprites;
        public float framerate;
        public TimeLine MainTimeline = new TimeLine();
        Dictionary<string, SpriteObject> SpriteDict = new Dictionary<string, SpriteObject>();
        Dictionary<string, TimeLine> SymDict = new Dictionary<string, TimeLine>();
        Dictionary<string, int> LayerState = new Dictionary<string, int>();
        Dictionary<string, int> FrameLevelDict = new Dictionary<string, int>();
        public Dictionary<string, LayerStateObject> LayerStateObjDICT = new Dictionary<string, LayerStateObject>();
        public int currentFrame = 0;
        public float currentTime = 0;
        //public float currentTimeforEnable = 0;
        //public int currentFrameforEnable = 0;

        public string mainGameObjectName;
        public LayerStateObject mainGameObject = new LayerStateObject();
        
        private string spritemapPath;
        private string folder;

        public MainAnimation(string resourceFolder) {
            folder = resourceFolder;
            spritemapPath = String.Format("{0}/spritemap", folder);
        }
        public void Start1()
        {
            SpriteSlicer();
            AnimationParser();
            Time.timeScale = framerate;
            AnimationPreprocessor();
            
        }
        public void SpriteSlicer()
        {
            ParsedSpriteMap spritemapjson;
            Texture2D texture = (Texture2D)Resources.Load<Texture2D>(spritemapPath) as Texture2D;
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.isReadable = true;
            ti.textureType = TextureImporterType.Sprite;

            List<SpriteMetaData> newData = new List<SpriteMetaData>();
            using (StreamReader r = new StreamReader(@"Assets/Resources/" + folder + "/spritemap.json"))
            {
                string json = r.ReadToEnd();
                spritemapjson = JsonUtility.FromJson<ParsedSpriteMap>(json);
            }

            foreach (var sprite in spritemapjson.ATLAS.SPRITES)
            {

                int SliceWidth = sprite.SPRITE.w;
                int SliceHeight = sprite.SPRITE.h;
                float pivotX = Mathf.Abs((SliceWidth - 0) / SliceWidth);
                float pivotY = Mathf.Abs((SliceHeight - 0) / SliceHeight);
                SpriteMetaData smd = new SpriteMetaData();
                smd.pivot = new Vector2((1 - pivotX), pivotY);
                smd.alignment = 9;
                smd.name = sprite.SPRITE.name;
                smd.rect = new Rect(sprite.SPRITE.x, spritemapjson.meta.size.h - sprite.SPRITE.y - SliceHeight, SliceWidth, SliceHeight);
                newData.Add(smd);
            }

            ti.spritesheet = newData.ToArray();
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.spritePixelsPerUnit = 1;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            texture.Apply(true);
            sprites = Resources.LoadAll<Sprite>(spritemapPath);

            foreach (var sp in sprites)
            {
                SpriteObject sobj = new SpriteObject();
                sobj.name = sp.name;
                sobj.sp = sp;
                sobj.rotated = false;
                SpriteDict.Add(sobj.name, sobj);
            }
            foreach (var sprites in spritemapjson.ATLAS.SPRITES)
            {
                if (sprites.SPRITE.rotated)
                {
                    SpriteObject sobj = SpriteDict[sprites.SPRITE.name];
                    sobj.rotated = true;
                    SpriteDict[sobj.name] = sobj;
                }
            }
        }
        public void AnimationParser()
        {
            using (StreamReader r = new StreamReader(@"Assets/Resources/" + folder + "/Animation.json"))
            {
                string json = r.ReadToEnd();
                ParsedAnimation animjson = JsonUtility.FromJson<ParsedAnimation>(json);
                framerate = animjson.metadata.framerate;
                mainGameObject.LayerStateName = animjson.ANIMATION.SYMBOL_name;
                if (animjson.ANIMATION.StageInstance.SYMBOL_Instance != null)
                    mainGameObject.arr[0] = animjson.ANIMATION.StageInstance.SYMBOL_Instance.DecomposedMatrix;
                MainTimeline = animjson.ANIMATION.TIMELINE;
                foreach (var layer in MainTimeline.LAYERS)
                {
                    foreach (var frame in layer.Frames)
                    {
                        if (layer.LayerDuration < frame.index + frame.duration)
                            layer.LayerDuration = frame.index + frame.duration;

                    }
                    if (MainTimeline.TimelineDuration < layer.LayerDuration)
                        MainTimeline.TimelineDuration = layer.LayerDuration;
                }
                if (animjson.SYMBOL_DICTIONARY.Symbols != null)
                {
                    foreach (var Symbols in animjson.SYMBOL_DICTIONARY.Symbols)
                    {
                        SymDict[Symbols.SYMBOL_name] = Symbols.TIMELINE;
                        foreach (var layer in Symbols.TIMELINE.LAYERS)
                        {
                            foreach (var frame in layer.Frames)
                            {
                                if (layer.LayerDuration < frame.index + frame.duration)
                                    layer.LayerDuration = frame.index + frame.duration;
                            }
                            if (Symbols.TIMELINE.TimelineDuration < layer.LayerDuration)
                                Symbols.TIMELINE.TimelineDuration = layer.LayerDuration;
                        }
                    }
                }
            }
        }
        public void AnimationPreprocessor()
        {
            int currentFrameIndex = 0;
            mainGameObject.gobj = new GameObject();
            mainGameObject.gobj.name = mainGameObject.LayerStateName;
            mainGameObject.parent = null;
            mainGameObject.sortingorder[currentFrameIndex] = 0;
            int Layercounter = 0;
            foreach (var layer in MainTimeline.LAYERS)
            {
                Layercounter = ProcessMainLayer(layer, currentFrameIndex, mainGameObject, Layercounter);
                Layercounter++;
            }
            LayerStateObjDICT[mainGameObject.LayerStateName] = mainGameObject;
        }
        public int ProcessMainLayer(Layer layer, int currentFrameIndex, LayerStateObject ParentLSO, int Layercounter)
        {
            int LayerDuration = layer.LayerDuration;
            string uniqueID = ParentLSO.LayerStateName + "_" + layer.Layer_name + Layercounter;
            LayerStateObject LSO = new LayerStateObject();
            LSO.gobj = new GameObject();
            LSO.gobj.name = uniqueID;
            LSO.LayerStateName = uniqueID;
            LSO.parent = ParentLSO;
            LSO.sortingorder[currentFrameIndex] = ParentLSO.sortingorder[currentFrameIndex] + Layercounter;
            ParentLSO.children.Add(LSO.LayerStateName);
            foreach (var frame in layer.Frames)
            {
                currentFrame = frame.index;
                Layercounter = ProcessMainFrame(frame, currentFrameIndex, uniqueID, LSO, Layercounter);
                currentFrameIndex += frame.duration;
            }
            LayerStateObjDICT[LSO.LayerStateName] = LSO;
            return Layercounter;
        }
        public int ProcessMainFrame(Frame frame, int currentFrameIndex, string uniqueID, LayerStateObject ParentLSO, int Layercounter)
        {
            int counter = 1;
            List<string> tempFrame = new List<string>();
            if (frame.name != null)
                FrameLevelDict[frame.name] = currentFrameIndex;
            int Elementcounter = Layercounter;
            for (int i = frame.elements.Length-1; i >= 0; i--)
            {
                var ele = frame.elements[i];
                if (ele.ATLAS_SPRITE_instance.name != null)
                {
                    string ID = uniqueID + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                    while (tempFrame.Contains(ID))
                    {
                        counter++;
                        ID = uniqueID + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                    }
                    tempFrame.Add(ID);
                    ProcessAtlasInstance(ele, frame.zDepth, currentFrameIndex, frame.duration, ID, ParentLSO, Elementcounter);
                    if (!ParentLSO.children.Contains(ID))
                        ParentLSO.children.Add(ID);
                }
                else if (ele.SYMBOL_Instance.SYMBOL_name != null)
                {
                    string ID = uniqueID + "_" + ele.SYMBOL_Instance.SYMBOL_name + ele.SYMBOL_Instance.Instance_Name + counter;
                    while (tempFrame.Contains(ID))
                    {
                        counter++;
                        ID = uniqueID + "_" + ele.SYMBOL_Instance.SYMBOL_name + ele.SYMBOL_Instance.Instance_Name + counter;
                    }
                    tempFrame.Add(ID);
                    Elementcounter = ProcessSymbolInstance(ele, frame.zDepth, currentFrameIndex, frame.duration, ID, ParentLSO, Elementcounter);
                    if (!ParentLSO.children.Contains(ID))
                        ParentLSO.children.Add(ID);
                }
                Elementcounter++;
            }
            Layercounter += frame.elements.Length;
            return Layercounter;
        }
        public void ProcessAtlasInstance(Element ele, float zDepth, int currentFrameIndex, int duration, string uniqueID, LayerStateObject ParentLSO, int counter)
        {

            LayerStateObject LSO;
            if (LayerStateObjDICT.ContainsKey(uniqueID))
            {
                LSO = LayerStateObjDICT[uniqueID];
            }
            else
            {
                LSO = new LayerStateObject();
                LSO.LayerStateName = uniqueID;
                LSO.spriteName = ele.ATLAS_SPRITE_instance.name;
                LSO.parent = ParentLSO;
                LSO.zDepth = zDepth;
                LSO.type = "atlas";
                LSO.gobj = new GameObject();
                LSO.gobj.name = uniqueID;
                LSO.anim = LSO.gobj.AddComponent<Animation>();
            }
            int index = currentFrameIndex;
            while (index < (currentFrameIndex + duration))
            {
                decomposedMat decomposed = new decomposedMat();
                decomposed.Position = ele.ATLAS_SPRITE_instance.Position;
                LSO.arr[index] = decomposed;

                LSO.sortingorder[index] = counter;

                index++;
            }
            LayerStateObjDICT[LSO.LayerStateName] = LSO;
        }
        public int ProcessSymbolInstance(Element ele, float zDepth, int currentFrameIndex, int duration, string uniqueSymbolID, LayerStateObject ParentLSO, int counter)
        {
            LayerStateObject LSO;
            if (LayerStateObjDICT.ContainsKey(uniqueSymbolID))
                LSO = LayerStateObjDICT[uniqueSymbolID];
            else
            {
                LSO = new LayerStateObject();
                LSO.LayerStateName = uniqueSymbolID;
                LSO.parent = ParentLSO;
                LSO.gobj = new GameObject();
                LSO.gobj.name = uniqueSymbolID;
                LSO.anim = LSO.gobj.AddComponent<Animation>();
            }
            int index = currentFrameIndex;
            while (index < (currentFrameIndex + duration))
            {
                decomposedMat decomposed = new decomposedMat();
                decomposed = ele.SYMBOL_Instance.DecomposedMatrix;
                LSO.arr[index] = decomposed;
                LSO.sortingorder[index] = counter;
                index++;
            }
            if (ele.SYMBOL_Instance.bitmap.name != null)
            {
                string uniqueBitmap = uniqueSymbolID + ele.SYMBOL_Instance.bitmap.name;

                LayerStateObject LSObitmap;
                if (LayerStateObjDICT.ContainsKey(uniqueBitmap))
                {
                    LSObitmap = LayerStateObjDICT[uniqueBitmap];
                }
                else
                {
                    LSObitmap = new LayerStateObject();
                    LSObitmap.LayerStateName = uniqueBitmap;
                    LSObitmap.spriteName = ele.SYMBOL_Instance.bitmap.name;
                    LSObitmap.parent = ParentLSO;
                    LSObitmap.zDepth = zDepth;
                    LSObitmap.type = "bitmap";
                    LSObitmap.gobj = new GameObject();
                    LSObitmap.gobj.name = uniqueBitmap;
                    LSObitmap.anim = LSObitmap.gobj.AddComponent<Animation>();
                    LSO.children.Add(uniqueBitmap);
                }
                index = currentFrameIndex;
                while (index < (currentFrameIndex + duration))
                {
                    decomposedMat decomposed = new decomposedMat();
                    decomposed.Position = ele.SYMBOL_Instance.bitmap.Position;
                    decomposed.Scaling = new Vector3(1, 1, 1);
                    LSObitmap.arr[index] = decomposed;
                    LSObitmap.sortingorder[index] = counter;
                    if (ele.SYMBOL_Instance.color.mode == "Alpha")
                    {
                        Color32 c1 = new Color32();
                        float a = ele.SYMBOL_Instance.color.alphaMultiplier * 255 + ele.SYMBOL_Instance.color.AlphaOffset;
                        Byte[] bytes = BitConverter.GetBytes((int)a);
                        c1.a = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.r = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.g = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.b = bytes[0];

                        LSObitmap.color[index] = c1;
                    }
                    else if (ele.SYMBOL_Instance.color.mode == "Advanced")
                    {
                        Color32 c1 = new Color32();
                        float temp = ele.SYMBOL_Instance.color.alphaMultiplier * 255 + ele.SYMBOL_Instance.color.AlphaOffset;
                        Byte[] bytes = BitConverter.GetBytes((int)temp);
                        c1.a = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.r = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.g = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.b = bytes[0];
                        LSObitmap.color[index] = c1;
                    }
                    else
                    {
                        Color32 c1 = new Color32();
                        Byte[] bytes = BitConverter.GetBytes((int)255);
                        c1.a = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.r = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.g = bytes[0];
                        bytes = BitConverter.GetBytes((int)255);
                        c1.b = bytes[0];
                        LSObitmap.color[index] = c1;
                    }
                    index++;
                }
                LayerStateObjDICT[LSObitmap.LayerStateName] = LSObitmap;
            }
            else
            {
                TimeLine nestedTimeline = SymDict[ele.SYMBOL_Instance.SYMBOL_name];

                if (ele.SYMBOL_Instance.symbolType == "movieclip")
                {
                    counter = ProcessMovieClip(ele, nestedTimeline, currentFrameIndex, duration, uniqueSymbolID, LSO, counter);
                }
                else if ((ele.SYMBOL_Instance.symbolType == "graphic") && (ele.SYMBOL_Instance.loop == "loop"))
                {
                    counter = ProcessGraphicLoop(ele, nestedTimeline, currentFrameIndex, duration, uniqueSymbolID, LSO, counter);
                }
                else if ((ele.SYMBOL_Instance.symbolType == "graphic") && (ele.SYMBOL_Instance.loop == "playonce"))
                {
                    counter = ProcessGraphicPlayOnce(ele, nestedTimeline, currentFrameIndex, duration, uniqueSymbolID, LSO, counter);
                }
                else if ((ele.SYMBOL_Instance.symbolType == "graphic") && (ele.SYMBOL_Instance.loop == "singleframe"))
                {
                    counter = ProcessGraphicSingleFrame(ele, nestedTimeline, currentFrameIndex, duration, uniqueSymbolID, LSO, counter);
                }
            }
            LayerStateObjDICT[LSO.LayerStateName] = LSO;
            return counter;
        }
        public int ProcessMovieClip(Element ele, TimeLine nestedTimeline, int currentFrameIndex, int duration, string uniqueSymbolID, LayerStateObject ParentLSO, int SymbolCounter)
        {
            int NestedFrameDuration = 0, finalDuration = 0;
            int loop = 0;
            int NestedFrameIndex;
            int loopvalue = 0;
            int loopvalue2 = 0;
            int loop2 = 0;
            int ElementCount = 0;
            int Layercounter = SymbolCounter;
            Dictionary<string, int> LayerStateTemp = new Dictionary<string, int>();

            do
            {
                foreach (var nestedlayer in nestedTimeline.LAYERS)
                {
                    LayerStateObject LSO;
                    if (LayerStateObjDICT.ContainsKey(uniqueSymbolID + nestedlayer.Layer_name))
                        LSO = LayerStateObjDICT[uniqueSymbolID + nestedlayer.Layer_name];
                    else
                    {
                        LSO = new LayerStateObject();
                        LSO.LayerStateName = uniqueSymbolID + nestedlayer.Layer_name;
                        LSO.parent = ParentLSO;
                        LSO.gobj = new GameObject();
                        LSO.gobj.name = LSO.LayerStateName;
                    }
                    if (!ParentLSO.children.Contains(LSO.LayerStateName))
                        ParentLSO.children.Add(LSO.LayerStateName);
                    string uniqueSymbolIDtemp = uniqueSymbolID + nestedlayer.Layer_name;
                    int firstframe = 0;
                    int frameindex = 0;
                    if (loop == 0)
                    {
                        LSO.sortingorder[currentFrameIndex] = Layercounter;

                        if (LayerState.ContainsKey(uniqueSymbolIDtemp))
                            firstframe = LayerState[uniqueSymbolIDtemp];
                        for (int i = 0; i < nestedlayer.Frames.Length; i++)
                        {
                            Frame frame = nestedlayer.Frames[i];
                            if ((firstframe >= frame.index) && (firstframe < frame.index + frame.duration))
                            {
                                frameindex = i;
                                break;
                            }
                        }
                    }
                    NestedFrameIndex = currentFrameIndex + (loop2);
                    for (int j = frameindex; j < nestedlayer.Frames.Length; j++)
                    {
                        List<string> tempFrame = new List<string>();
                        int counter = 1;
                        var frame = nestedlayer.Frames[j];
                        if (j == frameindex)
                            NestedFrameDuration = frame.duration + frame.index - firstframe;
                        else
                            NestedFrameDuration = frame.duration;

                        if ((NestedFrameIndex + NestedFrameDuration) < (currentFrameIndex + duration))
                            finalDuration = NestedFrameDuration;
                        else
                            finalDuration = (currentFrameIndex + duration) - NestedFrameIndex;


                        int nestedElementCounter = Layercounter;
                        if (ElementCount < frame.elements.Length)
                            ElementCount = frame.elements.Length;
                        for (int i = frame.elements.Length - 1; i >= 0; i--)
                        {
                            var nestedElement = frame.elements[i];
                            if (nestedElement.ATLAS_SPRITE_instance.name != null)
                            {
                                string ID = LSO.LayerStateName + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                                while (tempFrame.Contains(ID))
                                {
                                    counter++;
                                    ID = LSO.LayerStateName + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                                }
                                tempFrame.Add(ID);
                                ProcessAtlasInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                                if (!LSO.children.Contains(ID))
                                    LSO.children.Add(ID);
                            }
                            else if (nestedElement.SYMBOL_Instance.SYMBOL_name != null)
                            {
                                string ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                                while (tempFrame.Contains(ID))
                                {
                                    counter++;
                                    ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                                }
                                tempFrame.Add(ID);
                                nestedElementCounter=ProcessSymbolInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                                if (!LSO.children.Contains(ID))
                                    LSO.children.Add(ID);
                            }
                            nestedElementCounter++;
                        }
                        NestedFrameIndex += finalDuration;
                        if (loopvalue < NestedFrameIndex)
                        {
                            loopvalue = NestedFrameIndex;
                            loopvalue2 += finalDuration;
                        }
                        if (NestedFrameIndex >= (currentFrameIndex + duration))
                            break;
                    }
                    LayerStateTemp[uniqueSymbolIDtemp] = (NestedFrameIndex % nestedTimeline.TimelineDuration);
                    if (!LayerStateObjDICT.ContainsKey(LSO.LayerStateName))
                        LayerStateObjDICT[LSO.LayerStateName] = LSO;
                    Layercounter += ElementCount;
                }
                loop = loopvalue;
                loop2 = loopvalue2;
                foreach (var item in LayerStateTemp)
                {
                    LayerState[item.Key] = item.Value;
                }
            } while (loop < (currentFrameIndex + duration));
            return Layercounter;
        }
        public int ProcessGraphicLoop(Element ele, TimeLine nestedTimeline, int currentFrameIndex, int duration, string uniqueSymbolID, LayerStateObject ParentLSO, int SymbolCounter)
        {
            int NestedFrameDuration = 0, finalDuration = 0;
            int loop = 0;
            int NestedFrameIndex;
            int loopvalue = 0;
            int ElementCount = 0;
            int Layercounter = SymbolCounter;
            do
            {
                foreach (var nestedlayer in nestedTimeline.LAYERS)
                {
                    LayerStateObject LSO;
                    if (LayerStateObjDICT.ContainsKey(uniqueSymbolID + nestedlayer.Layer_name))
                        LSO = LayerStateObjDICT[uniqueSymbolID + nestedlayer.Layer_name];
                    else
                    {
                        LSO = new LayerStateObject();
                        LSO.LayerStateName = uniqueSymbolID + nestedlayer.Layer_name;
                        LSO.parent = ParentLSO;
                        LSO.gobj = new GameObject();
                        LSO.gobj.name = LSO.LayerStateName;
                    }
                    if (!ParentLSO.children.Contains(LSO.LayerStateName))
                        ParentLSO.children.Add(LSO.LayerStateName);

                    int firstframe = 0;
                    int frameindex = 0;
                    if (loop == 0)
                    {
                        LSO.sortingorder[currentFrameIndex] = Layercounter;
                        firstframe = ele.SYMBOL_Instance.firstFrame - 1;
                        for (int i = 0; i < nestedlayer.Frames.Length; i++)
                        {
                            Frame frame = nestedlayer.Frames[i];
                            if ((firstframe >= frame.index) && (firstframe < frame.index + frame.duration))
                            {
                                frameindex = i;
                                break;
                            }
                        }
                    }
                    NestedFrameIndex = currentFrameIndex + (loop);
                    for (int j = frameindex; j < nestedlayer.Frames.Length; j++)
                    {
                        List<string> tempFrame = new List<string>();
                        int counter = 1;
                        var frame = nestedlayer.Frames[j];
                        if (j == frameindex)
                            NestedFrameDuration = frame.duration + frame.index - firstframe;
                        else
                            NestedFrameDuration = frame.duration;

                        if ((NestedFrameIndex + NestedFrameDuration) < (currentFrameIndex + duration))
                            finalDuration = NestedFrameDuration;
                        else
                            finalDuration = (currentFrameIndex + duration) - NestedFrameIndex;

                        int nestedElementCounter = Layercounter;
                        if (ElementCount < frame.elements.Length)
                            ElementCount = frame.elements.Length;
                        for (int i = frame.elements.Length - 1; i >= 0; i--)
                        {
                            var nestedElement = frame.elements[i];
                            if (nestedElement.ATLAS_SPRITE_instance.name != null)
                            {
                                string ID = LSO.LayerStateName + "_" + nestedElement.ATLAS_SPRITE_instance.name + counter;
                                while (tempFrame.Contains(ID))
                                {
                                    counter++;
                                    ID = LSO.LayerStateName + "_" + nestedElement.ATLAS_SPRITE_instance.name + counter;
                                }
                                tempFrame.Add(ID);
                                ProcessAtlasInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                                if (!LSO.children.Contains(ID))
                                    LSO.children.Add(ID);
                            }
                            else if (nestedElement.SYMBOL_Instance.SYMBOL_name != null)
                            {
                                string ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                                while (tempFrame.Contains(ID))
                                {
                                    counter++;
                                    ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                                }
                                tempFrame.Add(ID);
                                nestedElementCounter = ProcessSymbolInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                                if (!LSO.children.Contains(ID))
                                    LSO.children.Add(ID);
                            }
                            nestedElementCounter++;
                        }
                        NestedFrameIndex += finalDuration;
                        if (loopvalue < NestedFrameIndex)
                            loopvalue = NestedFrameIndex;
                        if (NestedFrameIndex >= (currentFrameIndex + duration))
                            break;
                    }
                    if (!LayerStateObjDICT.ContainsKey(LSO.LayerStateName))
                        LayerStateObjDICT[LSO.LayerStateName] = LSO;
                    Layercounter += ElementCount;
                }
                loop = loopvalue;
            } while (loop < (currentFrameIndex + duration));
            return Layercounter;
        }
        public int ProcessGraphicPlayOnce(Element ele, TimeLine nestedTimeline, int currentFrameIndex, int duration, string uniqueSymbolID, LayerStateObject ParentLSO, int SymbolCounter)
        {
            int NestedFrameDuration = 0, finalDuration = 0;
            int NestedFrameIndex;
            int ElementCount = 0;
            int Layercounter = SymbolCounter;
            foreach (var nestedlayer in nestedTimeline.LAYERS)
            {
                LayerStateObject LSO;
                if (LayerStateObjDICT.ContainsKey(uniqueSymbolID + nestedlayer.Layer_name))
                    LSO = LayerStateObjDICT[uniqueSymbolID + nestedlayer.Layer_name];
                else
                {
                    LSO = new LayerStateObject();
                    LSO.LayerStateName = uniqueSymbolID + nestedlayer.Layer_name;
                    LSO.parent = ParentLSO;
                    LSO.gobj = new GameObject();
                    LSO.gobj.name = LSO.LayerStateName;
                    LSO.sortingorder[currentFrameIndex] = Layercounter;
                }
                if (!ParentLSO.children.Contains(LSO.LayerStateName))
                    ParentLSO.children.Add(LSO.LayerStateName);
                int firstframe = ele.SYMBOL_Instance.firstFrame - 1;
                int frameindex = 0;
                for (int i = 0; i < nestedlayer.Frames.Length; i++)
                {
                    Frame frame = nestedlayer.Frames[i];
                    if ((firstframe >= frame.index) && (firstframe < frame.index + frame.duration))
                    {
                        frameindex = i;
                        break;
                    }
                }
                NestedFrameIndex = currentFrameIndex;
                for (int j = frameindex; j < nestedlayer.Frames.Length; j++)
                {
                    List<string> tempFrame = new List<string>();
                    int counter = 1;
                    var frame = nestedlayer.Frames[j];
                    if (j == frameindex)
                        NestedFrameDuration = frame.duration + frame.index - firstframe;
                    else
                        NestedFrameDuration = frame.duration;

                    if ((NestedFrameIndex + NestedFrameDuration) < (currentFrameIndex + duration))
                        finalDuration = NestedFrameDuration;
                    else
                        finalDuration = (currentFrameIndex + duration) - NestedFrameIndex;
                    int nestedElementCounter = Layercounter;
                    if (ElementCount < frame.elements.Length)
                        ElementCount = frame.elements.Length;
                    for (int i = frame.elements.Length - 1; i >=0 ; i--)
                    {
                        var nestedElement = frame.elements[i];
                        if (nestedElement.ATLAS_SPRITE_instance.name != null)
                        {
                            string ID = LSO.LayerStateName + "_" + nestedElement.ATLAS_SPRITE_instance.name + counter;
                            while (tempFrame.Contains(ID))
                            {
                                counter++;
                                ID = LSO.LayerStateName + "_" + nestedElement.ATLAS_SPRITE_instance.name + counter;
                            }
                            tempFrame.Add(ID);
                            ProcessAtlasInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                            if (!LSO.children.Contains(ID))
                                LSO.children.Add(ID);
                        }
                        else if (nestedElement.SYMBOL_Instance.SYMBOL_name != null)
                        {
                            string ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                            while (tempFrame.Contains(ID))
                            {
                                counter++;
                                ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                            }
                            tempFrame.Add(ID);
                            nestedElementCounter = ProcessSymbolInstance(nestedElement, frame.zDepth, NestedFrameIndex, finalDuration, ID, LSO, nestedElementCounter);
                            if (!LSO.children.Contains(ID))
                                LSO.children.Add(ID);
                        }
                        nestedElementCounter++;
                    }
                    NestedFrameIndex += NestedFrameDuration;
                    //                Debug.Log(finalDuration);
                    if (NestedFrameIndex >= (currentFrameIndex + duration))
                        break;
                }
                if (!LayerStateObjDICT.ContainsKey(LSO.LayerStateName))
                    LayerStateObjDICT[LSO.LayerStateName] = LSO;
                Layercounter += ElementCount;
            }
            return Layercounter;
        }
        public int ProcessGraphicSingleFrame(Element ele, TimeLine nestedTimeline, int currentFrameIndex, int duration, string uniqueSymbolID, LayerStateObject ParentLSO, int SymbolCounter)
        {
            int ElementCount = 0;
            int Layercounter = SymbolCounter;
            foreach (var nestedlayer in nestedTimeline.LAYERS)
            {
                LayerStateObject LSO;
                if (LayerStateObjDICT.ContainsKey(uniqueSymbolID + nestedlayer.Layer_name))
                    LSO = LayerStateObjDICT[uniqueSymbolID + nestedlayer.Layer_name];
                else
                {
                    LSO = new LayerStateObject();
                    LSO.LayerStateName = uniqueSymbolID + nestedlayer.Layer_name;
                    LSO.parent = ParentLSO;
                    LSO.gobj = new GameObject();
                    LSO.gobj.name = LSO.LayerStateName;
                    LSO.sortingorder[currentFrameIndex] = Layercounter;
                }
                if (!ParentLSO.children.Contains(LSO.LayerStateName))
                    ParentLSO.children.Add(LSO.LayerStateName);

                int firstframe = ele.SYMBOL_Instance.firstFrame - 1;
                Frame requiredframe = nestedlayer.Frames[0];
                foreach (var frame in nestedlayer.Frames)
                {
                    if ((firstframe >= frame.index) && (firstframe < frame.index + frame.duration))
                    {
                        requiredframe = frame;
                        break;
                    }
                }
                List<string> tempFrame = new List<string>();
                int counter = 1;
                int nestedElementCounter = Layercounter;
                if (ElementCount < requiredframe.elements.Length)
                    ElementCount = requiredframe.elements.Length;
                for (int i = requiredframe.elements.Length - 1; i >= 0; i--)
                {
                    var nestedElement = requiredframe.elements[i];
                    if (nestedElement.ATLAS_SPRITE_instance.name != null)
                    {
                        string ID = LSO.LayerStateName + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                        while (tempFrame.Contains(ID))
                        {
                            counter++;
                            ID = LSO.LayerStateName + "_" + ele.ATLAS_SPRITE_instance.name + counter;
                        }
                        tempFrame.Add(ID);
                        ProcessAtlasInstance(nestedElement, requiredframe.zDepth, currentFrameIndex, duration, ID, LSO, nestedElementCounter);
                        if (!LSO.children.Contains(ID))
                            LSO.children.Add(ID);
                    }
                    else if (nestedElement.SYMBOL_Instance.SYMBOL_name != null)
                    {
                        string ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                        while (tempFrame.Contains(ID))
                        {
                            counter++;
                            ID = LSO.LayerStateName + "_" + nestedElement.SYMBOL_Instance.SYMBOL_name + nestedElement.SYMBOL_Instance.Instance_Name + counter;
                        }
                        tempFrame.Add(ID);
                        nestedElementCounter=ProcessSymbolInstance(nestedElement, requiredframe.zDepth, currentFrameIndex, duration, ID, LSO, nestedElementCounter);
                        if (!LSO.children.Contains(ID))
                            LSO.children.Add(ID);
                    }
                    nestedElementCounter++;
                }
                if (!LayerStateObjDICT.ContainsKey(LSO.LayerStateName))
                    LayerStateObjDICT[LSO.LayerStateName] = LSO;
                Layercounter += ElementCount;
            }
            return Layercounter;
        }

        public void PlayAnimation(string framelevel)
        {
            CreateGameObject(mainGameObject.LayerStateName, null, framelevel);
            PlayGameObject(mainGameObject.LayerStateName, framelevel);
        }
        public void CreateGameObject(string name, GameObject Parent, string framelevel)
        {
            if (LayerStateObjDICT.ContainsKey(name))
            {
                LayerStateObject ls = LayerStateObjDICT[name];
                if (ls.spriteName != null)
                {
                    if (ls.gobj.GetComponent<SpriteRenderer>() == null)
                        ls.gobj.AddComponent<SpriteRenderer>();
                    SpriteObject sobj = SpriteDict[ls.spriteName];
                    ls.gobj.GetComponent<SpriteRenderer>().sprite = sobj.sp;
                    ls.gobj.GetComponent<SpriteRenderer>().enabled = false;
                    if (sobj.rotated)
                    {
                        ls.gobj.GetComponent<SpriteRenderer>().flipX = true;
                        ls.gobj.GetComponent<SpriteRenderer>().flipY = true;
                    }
                   
                }
                if (Parent != null)
                    ls.gobj.transform.parent = Parent.transform;
                if (name == mainGameObject.LayerStateName && mainGameObject.arr.Count != 0)
                {
                    mainGameObject.gobj.transform.position = mainGameObject.arr[0].Position;
                    mainGameObject.gobj.transform.rotation = Quaternion.Euler(mainGameObject.arr[0].Rotation * Mathf.Rad2Deg);
                    mainGameObject.gobj.transform.localScale = mainGameObject.arr[0].Scaling;
                }
                else if (ls.arr.Count > 0)
                {
                    AnimationCurve PositioncurveX = new AnimationCurve();
                    AnimationCurve PositioncurveY = new AnimationCurve();
                    AnimationCurve PositioncurveZ = new AnimationCurve();
                    AnimationCurve RotationcurveX = new AnimationCurve();
                    AnimationCurve RotationcurveY = new AnimationCurve();
                    AnimationCurve RotationcurveZ = new AnimationCurve();
                    AnimationCurve RotationcurveW = new AnimationCurve();
                    AnimationCurve ScalingcurveX = new AnimationCurve();
                    AnimationCurve ScalingcurveY = new AnimationCurve();
                    AnimationCurve ScalingcurveZ = new AnimationCurve();
                    AnimationClip clip = new AnimationClip();
                    clip.legacy = true;
                    int i = 0; bool PlayTimelineflag = true;
                    if (FrameLevelDict.Count == 0)
                        i = 0;
                    else if (FrameLevelDict.ContainsKey(framelevel))
                    {
                        i = FrameLevelDict[framelevel];
                        PlayTimelineflag = false;
                    }
                    do
                    {
                        if (ls.arr.ContainsKey(i))
                        {
                            decomposedMat decomposed1 = ls.arr[i];
                            if (ls.spriteName != null)
                            {
                                SpriteObject sobj = SpriteDict[ls.spriteName];
                                if (sobj.rotated)
                                {
                                    ls.gobj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
                                    decomposed1.Position += new Vector3(0, sobj.sp.rect.width, 0);
                                }
                            }
                            PositioncurveX.AddKey(i, decomposed1.Position.x);
                            PositioncurveY.AddKey(i, -decomposed1.Position.y);
                            PositioncurveZ.AddKey(i, decomposed1.Position.z);

                            if (ls.spriteName == null)
                            {
                                Quaternion value = Quaternion.Euler(-decomposed1.Rotation * Mathf.Rad2Deg);
                                RotationcurveX.AddKey(i, value.x);
                                RotationcurveY.AddKey(i, value.y);
                                RotationcurveZ.AddKey(i, value.z);
                                RotationcurveW.AddKey(i, value.w);

                                ScalingcurveX.AddKey(i, (float)decomposed1.Scaling.x);
                                ScalingcurveY.AddKey(i, (float)decomposed1.Scaling.y);
                                ScalingcurveZ.AddKey(i, (float)decomposed1.Scaling.z);
                            }
                            else
                            {
                                ScalingcurveX.AddKey(i, 1);
                                ScalingcurveY.AddKey(i, 1);
                                ScalingcurveZ.AddKey(i, 1);
                            }
                            ls.SpriteEnabled[i] = true;
                        }
                        else
                        {
                            ScalingcurveX.AddKey(i, (float)1);
                            ScalingcurveY.AddKey(i, (float)1);
                            ScalingcurveZ.AddKey(i, (float)1);
                            ls.SpriteEnabled[i] = false;
                        }
                    } while ((!FrameLevelDict.ContainsValue(++i) || PlayTimelineflag) && i < MainTimeline.TimelineDuration);
                    clip.SetCurve("", typeof(Transform), "localPosition.x", PositioncurveX);
                    clip.SetCurve("", typeof(Transform), "localPosition.y", PositioncurveY);
                    clip.SetCurve("", typeof(Transform), "localPosition.z", PositioncurveZ);
                    if (ls.spriteName == null)
                    {
                        clip.SetCurve("", typeof(Transform), "localRotation.x", RotationcurveX);
                        clip.SetCurve("", typeof(Transform), "localRotation.y", RotationcurveY);
                        clip.SetCurve("", typeof(Transform), "localRotation.z", RotationcurveZ);
                        clip.SetCurve("", typeof(Transform), "localRotation.w", RotationcurveW);
                    }
                    clip.SetCurve("", typeof(Transform), "localScale.x", ScalingcurveX);
                    clip.SetCurve("", typeof(Transform), "localScale.y", ScalingcurveY);
                    clip.SetCurve("", typeof(Transform), "localScale.z", ScalingcurveZ);
                    ls.anim.AddClip(clip, framelevel);
                    ls.anim.wrapMode = WrapMode.Loop;
                }

                foreach (var child in ls.children)
                {
                    CreateGameObject(child, ls.gobj, framelevel);
                }
            }
        }
        public void PlayGameObject(string name, string framelevel)
        {
            if (LayerStateObjDICT.ContainsKey(name))
            {
                LayerStateObject ls = LayerStateObjDICT[name];
                GameObject gobj = ls.gobj;
                if (ls.anim != null)
                    ls.anim.Play(framelevel);

                foreach (var child in ls.children)
                {
                    PlayGameObject(child, framelevel);
                }
            }
        }

    }
}
