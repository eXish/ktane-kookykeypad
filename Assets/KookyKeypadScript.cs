using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class KookyKeypadScript : MonoBehaviour
{

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMColorblindMode colorblind;

    public KMSelectable[] buttons;
    public GameObject[] buttonObjs;
    public TextMesh[] buttonTexts;
    public TextMesh[] colorblindTexts;

    public Renderer[] leds;
    public Material[] mats;

    private string[] symbols = { "❦", "➵", "✪", "ㅌ", "ㄾ", "ㄿ", "ひ", "け", "℔", "℈", "℞", "ᵊ", "aɪ", "✮", "〄", "⋭", "➳", "˂", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    private int[] avalues = { 13, 11, 14, 18, 15, 13, 11, 12, 16, 18, 13, 12, 19, 14, 11, 17, 11, 15, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
    private int bvalue;

    private string[] assignedSyms = new string[4];
    private int[] assignedVals = new int[4];

    private string[][] colorcombos = new string[15][];
    private int correctindex;

    private bool[] step1states = { false, false, false, false };
    private bool[] step2states = { false, false, false, false };
    private bool[] currentstates = { false, false, false, false };

    private Coroutine timer;
    private double time;

    private bool firstpress = false;
    private bool colormode = false;
    private bool striking = false;

    private bool colorblindActive = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
    }

    void Start()
    {
        if (colorblind.ColorblindModeActive) {
            colorblindActive = true;
        }

        step1states = new bool[] { false, false, false, false };
        step2states = new bool[] { false, false, false, false };
        for (int i = 0; i < colorcombos.Length; i++)
        {
            colorcombos[i] = new string[] { "unlit", "unlit", "unlit", "unlit" };
        }
        step1();
        step2();
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            if(colormode != true && striking != true)
            {
                if(pressed != buttons[4])
                {
                    if (firstpress == false)
                    {
                        firstpress = true;
                        timer = StartCoroutine(timerThing());
                    }
                    else if (currentstates[Array.IndexOf(buttons, pressed)] != true)
                    {
                        time = 0;
                    }
                }
                if (pressed == buttons[0] && currentstates[0] != true)
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    StartCoroutine(buttonDown(0));
                }
                else if (pressed == buttons[1] && currentstates[1] != true)
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    StartCoroutine(buttonDown(1));
                }
                else if (pressed == buttons[2] && currentstates[2] != true)
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    StartCoroutine(buttonDown(2));
                }
                else if (pressed == buttons[3] && currentstates[3] != true)
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    StartCoroutine(buttonDown(3));
                }
                else if (pressed == buttons[4] && firstpress == true)
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    StopCoroutine(timer);
                    Debug.LogFormat("[Kooky Keypad #{0}] Submitted the following buttons...", moduleId);
                    Debug.LogFormat("[Kooky Keypad #{0}] Top Left: {1} | Top Right: {2} | Bottom Left: {3} | Bottom Right: {4}", moduleId, currentstates[0], currentstates[1], currentstates[2], currentstates[3]);
                    if (checkBoolsFinal())
                    {
                        Debug.LogFormat("[Kooky Keypad #{0}] The submitted buttons were correct! Module Disarmed!", moduleId);
                        leds[0].material = mats[12];
                        leds[1].material = mats[12];
                        leds[2].material = mats[12];
                        leds[3].material = mats[12];
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                    }
                    else
                    {
                        Debug.LogFormat("[Kooky Keypad #{0}] The submitted buttons were incorrect! Strike! Module Resetting...", moduleId);
                        StartCoroutine(strike());
                        GetComponent<KMBombModule>().HandleStrike();
                    }
                }
            }
        }
    }

    private void step1()
    {
        Debug.LogFormat("[Kooky Keypad #{0}] -Step 1-", moduleId);
        bool good = false;
        while (good == false)
        {
            int rand1 = UnityEngine.Random.Range(1, 101);
            int choose1 = UnityEngine.Random.Range(0, 18);
            assignedSyms[0] = symbols[choose1];
            assignedVals[0] = avalues[choose1];
            if (rand1 <= 20)
            {
                int choose2 = UnityEngine.Random.Range(18, symbols.Length);
                assignedSyms[0] = symbols[choose2];
                assignedVals[0] = avalues[choose2];
            }
            int rand2 = UnityEngine.Random.Range(1, 101);
            int choose3 = UnityEngine.Random.Range(0, 18);
            assignedSyms[1] = symbols[choose3];
            assignedVals[1] = avalues[choose3];
            if (rand2 <= 20)
            {
                int choose4 = UnityEngine.Random.Range(18, symbols.Length);
                assignedSyms[1] = symbols[choose4];
                assignedVals[1] = avalues[choose4];
            }
            int rand3 = UnityEngine.Random.Range(1, 101);
            int choose5 = UnityEngine.Random.Range(0, 18);
            assignedSyms[2] = symbols[choose5];
            assignedVals[2] = avalues[choose5];
            if (rand3 <= 20)
            {
                int choose6 = UnityEngine.Random.Range(18, symbols.Length);
                assignedSyms[2] = symbols[choose6];
                assignedVals[2] = avalues[choose6];
            }
            int rand4 = UnityEngine.Random.Range(1, 101);
            int choose7 = UnityEngine.Random.Range(0, 18);
            assignedSyms[3] = symbols[choose7];
            assignedVals[3] = avalues[choose7];
            if (rand4 <= 20)
            {
                int choose8 = UnityEngine.Random.Range(18, symbols.Length);
                assignedSyms[3] = symbols[choose8];
                assignedVals[3] = avalues[choose8];
            }
            good = checkVals();
        }
        for (int i = 0; i < 4; i++)
        {
            buttonTexts[i].text = assignedSyms[i];
            if (i == 0)
            {
                Debug.LogFormat("[Kooky Keypad #{0}] The top left button has the symbol {1} (a value = {2})", moduleId, assignedSyms[i], assignedVals[i]);
            }
            else if (i == 1)
            {
                Debug.LogFormat("[Kooky Keypad #{0}] The top right button has the symbol {1} (a value = {2})", moduleId, assignedSyms[i], assignedVals[i]);
            }
            else if (i == 2)
            {
                Debug.LogFormat("[Kooky Keypad #{0}] The bottom left button has the symbol {1} (a value = {2})", moduleId, assignedSyms[i], assignedVals[i]);
            }
            else if (i == 3)
            {
                Debug.LogFormat("[Kooky Keypad #{0}] The bottom right button has the symbol {1} (a value = {2})", moduleId, assignedSyms[i], assignedVals[i]);
            }
        }
        if (bomb.IsPortPresent("HDMI") || bomb.IsPortPresent("RJ45"))
        {
            Debug.LogFormat("[Kooky Keypad #{0}] An HDMI or RJ-45 port is present! The b value is the first serial # digit which is {1}", moduleId, bvalue);
        }
        else
        {
            Debug.LogFormat("[Kooky Keypad #{0}] An HDMI or RJ-45 port is NOT present! The b value is the last serial # digit which is {1}", moduleId, bvalue);
        }

        if(assignedVals[0] + bvalue >= 20)
        {
            step1states[0] = true;
            Debug.LogFormat("[Kooky Keypad #{0}] Top Left: A + B -> {1} + {2} = {3} >= 20 | Press this button for step 2: {4}", moduleId, assignedVals[0], bvalue, assignedVals[0]+bvalue, step1states[0]);
        }
        else
        {
            Debug.LogFormat("[Kooky Keypad #{0}] Top Left: A + B -> {1} + {2} = {3} < 20 | Press this button for step 2: {4}", moduleId, assignedVals[0], bvalue, assignedVals[0] + bvalue, step1states[0]);
        }
        if (assignedVals[1] + bvalue >= 20)
        {
            step1states[1] = true;
            Debug.LogFormat("[Kooky Keypad #{0}] Top Right: A + B -> {1} + {2} = {3} >= 20 | Press this button for step 2: {4}", moduleId, assignedVals[1], bvalue, assignedVals[1] + bvalue, step1states[1]);
        }
        else
        {
            Debug.LogFormat("[Kooky Keypad #{0}] Top Right: A + B -> {1} + {2} = {3} < 20 | Press this button for step 2: {4}", moduleId, assignedVals[1], bvalue, assignedVals[1] + bvalue, step1states[1]);
        }
        if (assignedVals[2] + bvalue >= 20)
        {
            step1states[2] = true;
            Debug.LogFormat("[Kooky Keypad #{0}] Bottom Left: A + B -> {1} + {2} = {3} >= 20 | Press this button for step 2: {4}", moduleId, assignedVals[2], bvalue, assignedVals[2] + bvalue, step1states[2]);
        }
        else
        {
            Debug.LogFormat("[Kooky Keypad #{0}] Bottom Left: A + B -> {1} + {2} = {3} < 20 | Press this button for step 2: {4}", moduleId, assignedVals[2], bvalue, assignedVals[2] + bvalue, step1states[2]);
        }
        if (assignedVals[3] + bvalue >= 20)
        {
            step1states[3] = true;
            Debug.LogFormat("[Kooky Keypad #{0}] Bottom Right: A + B -> {1} + {2} = {3} >= 20 | Press this button for step 2: {4}", moduleId, assignedVals[3], bvalue, assignedVals[3] + bvalue, step1states[3]);
        }
        else
        {
            Debug.LogFormat("[Kooky Keypad #{0}] Bottom Right: A + B -> {1} + {2} = {3} < 20 | Press this button for step 2: {4}", moduleId, assignedVals[3], bvalue, assignedVals[3] + bvalue, step1states[3]);
        }
    }

    private void step2()
    {
        Debug.LogFormat("[Kooky Keypad #{0}] -Step 2-", moduleId);
        string[] colors = { "crimson", "red", "coral", "orange", "lemonchiffon", "mediumspringgreen", "deepseagreen", "cadetblue", "slateblue", "darkmagenta", "unlit", "unlit", "unlit" };
        for(int i = 0; i < colorcombos.Length; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                colorcombos[i][j] = colors[UnityEngine.Random.Range(0, colors.Length)];
            }
        }
        if (checkBools1( new bool[] { true, false, false, false }))
        {
            correctindex = 0;
        }
        else if (checkBools1( new bool[] { false, true, false, false }))
        {
            correctindex = 1;
        }
        else if (checkBools1( new bool[] { false, false, true, false }))
        {
            correctindex = 2;
        }
        else if (checkBools1( new bool[] { false, false, false, true }))
        {
            correctindex = 3;
        }
        else if (checkBools1( new bool[] { true, false, true, false }))
        {
            correctindex = 4;
        }
        else if (checkBools1( new bool[] { true, true, false, false }))
        {
            correctindex = 5;
        }
        else if (checkBools1( new bool[] { false, true, true, false }))
        {
            correctindex = 6;
        }
        else if (checkBools1( new bool[] { false, false, true, true }))
        {
            correctindex = 7;
        }
        else if (checkBools1( new bool[] { true, true, true, false }))
        {
            correctindex = 8;
        }
        else if (checkBools1( new bool[] { true, true, false, true }))
        {
            correctindex = 9;
        }
        else if (checkBools1( new bool[] { false, true, true, true }))
        {
            correctindex = 10;
        }
        else if (checkBools1( new bool[] { true, false, true, true }))
        {
            correctindex = 11;
        }
        else if (checkBools1( new bool[] { true, false, false, true }))
        {
            correctindex = 12;
        }
        else if (checkBools1( new bool[] { false, true, false, true }))
        {
            correctindex = 13;
        }
        else if (checkBools1( new bool[] { true, true, true, true }))
        {
            correctindex = 14;
        }
        Debug.LogFormat("[Kooky Keypad #{0}] If the correct buttons are pressed from step 1, then the LEDs should be showing the following in 'color mode'...", moduleId);
        Debug.LogFormat("[Kooky Keypad #{0}] Top Left: {1} | Top Right: {2} | Bottom Left: {3} | Bottom Right: {4}", moduleId, colorcombos[correctindex][0], colorcombos[correctindex][1], colorcombos[correctindex][2], colorcombos[correctindex][3]);
        List<int> toggled = new List<int>();
        if (bomb.IsTwoFactorPresent())
        {
            toggleStates(new int[] { 0, 3 });
            toggled.Add(1);
        }
        if (colorCount() > 1 && (colorPos(2).Equals("coral") || colorPos(2).Equals("slateblue")))
        {
            toggleStates(new int[] { 1 });
            toggled.Add(2);
        }
        if (colorCount() < 3 || bomb.GetPortPlateCount() == 2)
        {
            toggleStates(new int[] { 2, 3 });
            toggled.Add(3);
        }
        if (colorDisp("mediumspringgreen") || colorDisp("red") || colorDisp("orange"))
        {
            toggleStates(new int[] { 0 });
            toggled.Add(4);
        }
        if (bomb.IsIndicatorPresent("SIG") || bomb.IsIndicatorPresent("TRN"))
        {
            toggleStates(new int[] { 1, 3 });
            toggled.Add(5);
        }
        if (colorCount() == 1)
        {
            toggleStates(new int[] { 2 });
            toggled.Add(6);
        }
        if (colorPos(1).Equals("darkmagenta") && colorPos(colorCount()).Equals("lemonchiffon"))
        {
            toggleStates(new int[] { 0, 1, 3 });
            toggled.Add(7);
        }
        if (colorDisp("mediumspringgreen") || colorDisp("deepseagreen"))
        {
            toggleStates(new int[] { 0, 2 });
            toggled.Add(8);
        }
        if (!colorDisp("cadetblue") || !colorDisp("deepseagreen"))
        {
            toggleStates(new int[] { 1 });
            toggled.Add(9);
        }
        if(colorCount() == 0)
        {
            toggleStates(new int[] { 1, 2 });
            toggled.Add(10);
        }
        if (colorCount() != 4)
        {
            toggleStates(new int[] { 3 });
            toggled.Add(11);
        }
        if (checkBools3(new bool[] { false, false, false, false }))
        {
            toggleStates(new int[] { 1 });
            toggled.Add(12);
        }
        string build = "";
        for(int i = 0; i < toggled.Count; i++)
        {
            if(i == toggled.Count - 1)
            {
                build += "" + toggled.ElementAt(i);
            }
            else
            {
                build += toggled.ElementAt(i) + ", ";
            }
        }
        Debug.LogFormat("[Kooky Keypad #{0}] The used rules were: [ {1} ]", moduleId, build);
        Debug.LogFormat("[Kooky Keypad #{0}] After applying the rules the correct buttons to submit are...", moduleId);
        Debug.LogFormat("[Kooky Keypad #{0}] Top Left: {1} | Top Right: {2} | Bottom Left: {3} | Bottom Right: {4}", moduleId, step2states[0], step2states[1], step2states[2], step2states[3]);
    }

    private void toggleStates(int[] i)
    {
        for(int j = 0; j < i.Length; j++)
        {
            if(step2states[i[j]] == true)
            {
                step2states[i[j]] = false;
            }
            else
            {
                step2states[i[j]] = true;
            }
        }
    }

    private int colorCount()
    {
        int count = 0;
        for(int i = 0; i < 4; i++)
        {
            if (!colorcombos[correctindex][i].Equals("unlit"))
            {
                count++;
            }
        }
        return count;
    }

    private string colorPos(int n)
    {
        string col = "";
        int counter = 0;
        for (int i = 0; i < 4; i++)
        {
            if (!colorcombos[correctindex][i].Equals("unlit"))
            {
                counter++;
                if(counter == n)
                {
                    col = colorcombos[correctindex][i];
                    break;
                }
            }
        }
        return col;
    }

    private bool colorDisp(string s)
    {
        for (int i = 0; i < 4; i++)
        {
            if (colorcombos[correctindex][i].Equals(s))
            {
                return true;
            }
        }
        return false;
    }

    private bool checkBools1(bool[] b)
    {
        if (b[0] == step1states[0] && b[1] == step1states[1] && b[2] == step1states[2] && b[3] == step1states[3])
        {
            return true;
        }
        return false;
    }

    private bool checkBools2(bool[] b)
    {
        if (b[0] == currentstates[0] && b[1] == currentstates[1] && b[2] == currentstates[2] && b[3] == currentstates[3])
        {
            return true;
        }
        return false;
    }

    private bool checkBools3(bool[] b)
    {
        if (b[0] == step2states[0] && b[1] == step2states[1] && b[2] == step2states[2] && b[3] == step2states[3])
        {
            return true;
        }
        return false;
    }

    private bool checkBoolsFinal()
    {
        if (currentstates[0] == step2states[0] && currentstates[1] == step2states[1] && currentstates[2] == step2states[2] && currentstates[3] == step2states[3])
        {
            return true;
        }
        return false;
    }

    private IEnumerator strike()
    {
        striking = true;
        leds[0].material = mats[11];
        leds[1].material = mats[11];
        leds[2].material = mats[11];
        leds[3].material = mats[11];
        yield return new WaitForSeconds(1.0f);
        firstpress = false;
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = mats[10];
            if (currentstates[i] == true)
            {
                StartCoroutine(buttonUp(i));
            }
        }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        Start();
        striking = false;
        StopCoroutine("strike");
    }

    private IEnumerator timerThing()
    {
        time = 0;
        while (time < 1.25)
        {
            yield return new WaitForSeconds(0.01f);
            time += 0.01;
        }
        colormode = true;
        StartCoroutine(cmode());
        StopCoroutine(timer);
    }

    private IEnumerator cmode()
    {
        int index = 0;
        if (checkBools2( new bool[] { true, false, false, false }))
        {
            index = 0;
        }
        else if (checkBools2( new bool[] { false, true, false, false }))
        {
            index = 1;
        }
        else if (checkBools2( new bool[] { false, false, true, false }))
        {
            index = 2;
        }
        else if (checkBools2( new bool[] { false, false, false, true }))
        {
            index = 3;
        }
        else if (checkBools2( new bool[] { true, false, true, false }))
        {
            index = 4;
        }
        else if (checkBools2( new bool[] { true, true, false, false }))
        {
            index = 5;
        }
        else if (checkBools2( new bool[] { false, true, true, false }))
        {
            index = 6;
        }
        else if (checkBools2( new bool[] { false, false, true, true }))
        {
            index = 7;
        }
        else if (checkBools2( new bool[] { true, true, true, false }))
        {
            index = 8;
        }
        else if (checkBools2( new bool[] { true, true, false, true }))
        {
            index = 9;
        }
        else if (checkBools2( new bool[] { false, true, true, true }))
        {
            index = 10;
        }
        else if (checkBools2( new bool[] { true, false, true, true }))
        {
            index = 11;
        }
        else if (checkBools2( new bool[] { true, false, false, true }))
        {
            index = 12;
        }
        else if (checkBools2( new bool[] { false, true, false, true }))
        {
            index = 13;
        }
        else if (checkBools2( new bool[] { true, true, true, true }))
        {
            index = 14;
        }
        for (int i = 0; i < 4; i++)
        {
            if (colorcombos[index][i].Equals("crimson"))
            {
                leds[i].material = mats[0];
                if (colorblindActive) {colorblindTexts[i].text = "CRIM";}
            }
            else if (colorcombos[index][i].Equals("red"))
            {
                leds[i].material = mats[1];
                if (colorblindActive) {colorblindTexts[i].text = "RED";}
            }
            else if (colorcombos[index][i].Equals("coral"))
            {
                leds[i].material = mats[2];
                if (colorblindActive) {colorblindTexts[i].text = "CORAL";}
            }
            else if (colorcombos[index][i].Equals("orange"))
            {
                leds[i].material = mats[3];
                if (colorblindActive) {colorblindTexts[i].text = "ORANGE";}
            }
            else if (colorcombos[index][i].Equals("lemonchiffon"))
            {
                leds[i].material = mats[4];
                if (colorblindActive) {colorblindTexts[i].text = "LEMON";}
            }
            else if (colorcombos[index][i].Equals("mediumspringgreen"))
            {
                leds[i].material = mats[5];
                if (colorblindActive) {colorblindTexts[i].text = "MEDIUM";}
            }
            else if (colorcombos[index][i].Equals("deepseagreen"))
            {
                leds[i].material = mats[6];
                if (colorblindActive) {colorblindTexts[i].text = "DEEP";}
            }
            else if (colorcombos[index][i].Equals("cadetblue"))
            {
                leds[i].material = mats[7];
                if (colorblindActive) {colorblindTexts[i].text = "CADET";}
            }
            else if (colorcombos[index][i].Equals("slateblue"))
            {
                leds[i].material = mats[8];
                if (colorblindActive) {colorblindTexts[i].text = "SLATE";}
            }
            else if (colorcombos[index][i].Equals("darkmagenta"))
            {
                leds[i].material = mats[9];
                if (colorblindActive) {colorblindTexts[i].text = "DARK";}
            }
        }
        yield return new WaitForSeconds(2.0f);
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = mats[10];
            if (colorblindActive) {colorblindTexts[i].text = "";}
            if (currentstates[i] == true)
            {
                StartCoroutine(buttonUp(i));
            }
        }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        colormode = false;
        firstpress = false;
        StopCoroutine("cmode");
    }

    private IEnumerator buttonDown(int i)
    {
        if (i == 0)
        {
            currentstates[0] = true;
        }
        else if(i == 1)
        {
            currentstates[1] = true;
        }
        else if (i == 2)
        {
            currentstates[2] = true;
        }
        else if (i == 3)
        {
            currentstates[3] = true;
        }
        yield return new WaitForSeconds(0.1f);
        int movement = 0;
        while (movement != 10)
        {
            yield return new WaitForSeconds(0.007f);
            buttonObjs[i].transform.localPosition = buttonObjs[i].transform.localPosition + Vector3.up * -0.001f;
            movement++;
        }
        StopCoroutine("buttonDown");
    }

    private IEnumerator buttonUp(int i)
    {
        if (colorblindActive) {colorblindTexts[i].text = "";}
        if (i == 0)
        {
            currentstates[0] = false;
        }
        else if (i == 1)
        {
            currentstates[1] = false;
        }
        else if (i == 2)
        {
            currentstates[2] = false;
        }
        else if (i == 3)
        {
            currentstates[3] = false;
        }
        yield return new WaitForSeconds(0.1f);
        int movement = 0;
        while (movement != 10)
        {
            yield return new WaitForSeconds(0.007f);
            buttonObjs[i].transform.localPosition = buttonObjs[i].transform.localPosition + Vector3.up * 0.001f;
            movement++;
        }
        StopCoroutine("buttonUp");
    }

    private bool checkVals()
    {
        bvalue = 0;
        if(bomb.IsPortPresent("HDMI") || bomb.IsPortPresent("RJ45"))
        {
            bvalue = bomb.GetSerialNumberNumbers().ElementAt(0);
        }
        else
        {
            bvalue = bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count()-1);
        }
        if ((assignedVals[0]+ bvalue) >= 20)
        {
            return true;
        }
        else if ((assignedVals[1] + bvalue) >= 20)
        {
            return true;
        }
        else if ((assignedVals[2] + bvalue) >= 20)
        {
            return true;
        }
        else if ((assignedVals[3] + bvalue) >= 20)
        {
            return true;
        }
        return false;
    }

    //twitch plays
    private bool isCmdValid(string cmd)
    {
        char[] valids = { '1', '2', '3', '4' };
        if ((cmd.Length >= 1) || (cmd.Length <= 4))
        {
            foreach (char c in cmd)
            {
                if (!valids.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <buttons> [Presses the specified buttons] | !{0} submit <buttons> [Submits the specified buttons] | !{0} colorblind [Toggles colorblind mode] | Button reference: 1=topleft, 2=topright, 3=bottomleft, 4=bottomright";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (colorblindActive)
            {
                colorblindActive = false;
                if (colormode)
                {
                    for (int i = 0; i < 4; i++)
                        colorblindTexts[i].text = "";
                }
            }
            else
            {
                colorblindActive = true;
                if (colormode)
                {
                    int index = 0;
                    if (checkBools2(new bool[] { true, false, false, false }))
                    {
                        index = 0;
                    }
                    else if (checkBools2(new bool[] { false, true, false, false }))
                    {
                        index = 1;
                    }
                    else if (checkBools2(new bool[] { false, false, true, false }))
                    {
                        index = 2;
                    }
                    else if (checkBools2(new bool[] { false, false, false, true }))
                    {
                        index = 3;
                    }
                    else if (checkBools2(new bool[] { true, false, true, false }))
                    {
                        index = 4;
                    }
                    else if (checkBools2(new bool[] { true, true, false, false }))
                    {
                        index = 5;
                    }
                    else if (checkBools2(new bool[] { false, true, true, false }))
                    {
                        index = 6;
                    }
                    else if (checkBools2(new bool[] { false, false, true, true }))
                    {
                        index = 7;
                    }
                    else if (checkBools2(new bool[] { true, true, true, false }))
                    {
                        index = 8;
                    }
                    else if (checkBools2(new bool[] { true, true, false, true }))
                    {
                        index = 9;
                    }
                    else if (checkBools2(new bool[] { false, true, true, true }))
                    {
                        index = 10;
                    }
                    else if (checkBools2(new bool[] { true, false, true, true }))
                    {
                        index = 11;
                    }
                    else if (checkBools2(new bool[] { true, false, false, true }))
                    {
                        index = 12;
                    }
                    else if (checkBools2(new bool[] { false, true, false, true }))
                    {
                        index = 13;
                    }
                    else if (checkBools2(new bool[] { true, true, true, true }))
                    {
                        index = 14;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (colorcombos[index][i].Equals("crimson"))
                        {
                            colorblindTexts[i].text = "CRIM";
                        }
                        else if (colorcombos[index][i].Equals("red"))
                        {
                            colorblindTexts[i].text = "RED";
                        }
                        else if (colorcombos[index][i].Equals("coral"))
                        {
                            colorblindTexts[i].text = "CORAL";
                        }
                        else if (colorcombos[index][i].Equals("orange"))
                        {
                            colorblindTexts[i].text = "ORANGE";
                        }
                        else if (colorcombos[index][i].Equals("lemonchiffon"))
                        {
                            colorblindTexts[i].text = "LEMON";
                        }
                        else if (colorcombos[index][i].Equals("mediumspringgreen"))
                        {
                            colorblindTexts[i].text = "MEDIUM";
                        }
                        else if (colorcombos[index][i].Equals("deepseagreen"))
                        {
                            colorblindTexts[i].text = "DEEP";
                        }
                        else if (colorcombos[index][i].Equals("cadetblue"))
                        {
                            colorblindTexts[i].text = "CADET";
                        }
                        else if (colorcombos[index][i].Equals("slateblue"))
                        {
                            colorblindTexts[i].text = "SLATE";
                        }
                        else if (colorcombos[index][i].Equals("darkmagenta"))
                        {
                            colorblindTexts[i].text = "DARK";
                        }
                    }
                }
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length >= 2)
            {
                string sub = command.Substring(6);
                sub = sub.Replace(" ", "");
                parameters[1] = sub;
                if (isCmdValid(parameters[1]))
                {
                    while (currentstates[0] == true || currentstates[1] == true || currentstates[2] == true || currentstates[3] == true) { yield return new WaitForSeconds(0.1f); }
                    for (int i = 0; i < parameters[1].Length; i++)
                    {
                        if (parameters[1].ElementAt(i).Equals('1'))
                        {
                            buttons[0].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('2'))
                        {
                            buttons[1].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('3'))
                        {
                            buttons[2].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('4'))
                        {
                            buttons[3].OnInteract();
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return "sendtochaterror The specified set of buttons to press is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the button(s) to press!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length >= 2)
            {
                string sub = command.Substring(7);
                sub = sub.Replace(" ", "");
                parameters[1] = sub;
                if (isCmdValid(parameters[1]))
                {
                    while (currentstates[0] == true || currentstates[1] == true || currentstates[2] == true || currentstates[3] == true) { yield return new WaitForSeconds(0.1f); }
                    for (int i = 0; i < parameters[1].Length; i++)
                    {
                        if (parameters[1].ElementAt(i).Equals('1'))
                        {
                            buttons[0].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('2'))
                        {
                            buttons[1].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('3'))
                        {
                            buttons[2].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('4'))
                        {
                            buttons[3].OnInteract();
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return new WaitForSeconds(0.5f);
                    buttons[4].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified set of buttons to submit is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the button(s) to submit!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        bool canSolve = true;
        if (!colormode)
        {
            for (int i = 0; i < 4; i++)
            {
                if (currentstates[i] == true && step2states[i] == false)
                {
                    canSolve = false;
                    break;
                }
            }
            if (canSolve)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (currentstates[i] != step2states[i])
                    {
                        buttons[i].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                yield return new WaitForSeconds(0.5f);
                buttons[4].OnInteract();
                yield break;
            }
        }
        while (currentstates[0] == true || currentstates[1] == true || currentstates[2] == true || currentstates[3] == true) { yield return true; }
        for (int i = 0; i < 4; i++)
        {
            if (currentstates[i] != step2states[i])
            {
                buttons[i].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(0.5f);
        buttons[4].OnInteract();
    }
}
