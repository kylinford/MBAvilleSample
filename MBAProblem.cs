using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

public class MBAProblem : JWMonoBehaviour {
    public Sprite tick;
    public Sprite cross;
    GameObject Content;
    MBAQuestElement_Solution solution;
    //MBAInventory inventory;
    MBADataBuffer db;

    bool showingHelp = false;
    bool submitted = false;
    int startMoney = 0;
    int startPrestige = 0;

    public bool Submitted { get { return submitted; }  }

    // Use this for initialization
    void Start() {
        db = ((GameObject)Resources.Load("Prefab/MBADataBuffer")).GetComponent<MBADataBuffer>();
        
        UpdateSubmitAvailability();
        startMoney = db.Inventory.Property.Money;
        startPrestige = db.Inventory.Property.Prestige;
    }

    void ToggleDowntownAndSkipButtons(bool b)
    {
        GameObject Button_Downtown = GameObject.Find("Button_BackToCityMap");
        GameObject Button_Skip = GameObject.Find("Button_Skip");
        Button_Downtown.GetComponent<Button>().enabled = b;
        Button_Skip.GetComponent<Button>().enabled = b;
    }

    // Update is called once per frame
    void Update() {
    }

    public void UpdateContent(string questID, MBAQuestElement_Problem problem)
    {
        StartCoroutine(UpdateContentCoroutine(questID, problem));
    }

    IEnumerator UpdateContentCoroutine(string questID, MBAQuestElement_Problem problem)
    {
        Content = transform.FindChild("Content").FindChild("Viewport").FindChild("Content").gameObject;
        Content.transform.localPosition = Vector3.zero;

        List<MBAQuestElement_CampItem> items = new List<MBAQuestElement_CampItem>();
        List<MBAQuestElement_Stock> stocks = new List<MBAQuestElement_Stock>();
        int blankCount = 0;
        MBAQuestElement_Choices choices_Temp = new MBAQuestElement_Choices(0.5f, -100, "x");

        GameObject Template_DDM = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_DDM");
        GameObject Template_DDMItem = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_DDMItem");
        GameObject Template_Help = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_ProblemHelp");
        GameObject Template_Text = Resources.Load<GameObject>("Prefab/UI/Text");
        GameObject Template_InputField = Resources.Load<GameObject>("Prefab/UI/InputField");
        GameObject Template_Image = Resources.Load<GameObject>("Prefab/UI/Image");
        GameObject Template_Toggle = Resources.Load<GameObject>("Prefab/UI/Toggle");
        GameObject Template_Table = Resources.Load<GameObject>("Prefab/JW/Table/JWPanel_Table");
        GameObject Template_Chart = Resources.Load<GameObject>("Prefab/JW/Chart/Panel_Chart");
        GameObject Template_Stock = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_Stock");
        GameObject Template_StockRecord = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_StockRecord");

        int objNumber = 0;
        foreach (object obj in problem.Content)
        {
            Type objType = obj.GetType();
            switch (objType.Name)
            {
                case "MBAQuestElement_Help":
                    if (FindObjectOfType<MBAProblem_Help>())
                        break;
                    GameObject newHelp = JWInstantiateUnderParent_UI(Template_Help, gameObject, true, new Vector3(660, -50, 0));
                    newHelp.GetComponent<MBAProblem_Help>().UpdateContent(questID, (MBAQuestElement_Help)obj);
                    break;
                case "MBAQuestElement_Stock":
                    MBAQuestElement_Stock stock = (MBAQuestElement_Stock)obj;
                    stocks.Add(stock);
                    break;
                case "MBAQuestElement_Blank":
                    //    <blank rightanswer="500" length="5" x="0.5" y="-550"></blank>
                    blankCount++;
                    MBAQuestElement_Blank blank = (MBAQuestElement_Blank)obj;
                    GameObject newInputField = JWInstantiateUnderParent_UI(Template_InputField, Content, "InputField", blankCount.ToString() + ".", true, CalculateLocalLocation(blank.Relative, blank.LocalPosition, Content), Vector3.one, Vector3.zero);
                    newInputField.GetComponent<InputField>().onValueChanged.AddListener(delegate { UpdateSubmitAvailability(); });
                    newInputField.GetComponent<MBAProblem_Blank>().RightAnswer = blank.RightAnswer;
                    newInputField.GetComponent<MBAProblem_Blank>().Margin = blank.Margin;
                    newInputField.GetComponent<MBAProblem_Blank>().isTemplate = false;
                    foreach (Text text in newInputField.GetComponentsInChildren<Text>())
                    {
                        //text.font = Resources.Load<Font>("Art/fonts/Belwe-Bold");
                        text.GetComponent<Text>().color = Color.black;
                    }
                    float blankWidth = blank.Width > 0 ? blank.Width : newInputField.GetComponent<RectTransform>().rect.width;
                    float blankHeight = blank.Height > 0 ? blank.Height : newInputField.GetComponent<RectTransform>().rect.height;
                    newInputField.GetComponent<RectTransform>().sizeDelta = new Vector2(blankWidth, blankHeight);
                    break;
                case "MBAQuestElement_Choices":
                    UpdateElement_Choices((MBAQuestElement_Choices)obj, Template_Toggle);
                    break;
                case "MBAQuestElement_Choice":
                    if (choices_Temp.Content.Count <= 0 && objNumber > 0)
                    {
                        MBAQuestElement lastElement = (MBAQuestElement)problem.Content[objNumber - 1];
                        choices_Temp = new MBAQuestElement_Choices(lastElement.LocalPosition.x, lastElement.LocalPosition.y - lastElement.Height/2.0f - 50, lastElement.Relative);
                    }
                    choices_Temp.Content.Add((MBAQuestElement_Choice)obj);
                    break;
                case "MBAQuestElement_Image":
                    MBAQuestElement_Image img = (MBAQuestElement_Image)obj;
                    GameObject newImage = JWInstantiateUnderParent_UI(Template_Image, Content, "Image", LoadSprite(img.Src, questID), true, CalculateLocalLocation(img.Relative, img.LocalPosition, Content), Vector3.one, Vector3.zero);
                    float imgWidth = img.Width > 0 ? img.Width : newImage.GetComponent<RectTransform>().rect.width;
                    float imgHeight = img.Height > 0 ? img.Height : newImage.GetComponent<RectTransform>().rect.height;
                    newImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth, imgHeight);
                    newImage.GetComponent<Image>().preserveAspect = true;
                    break;
                case "MBAQuestElement_Text":
                    Template_Text.GetComponent<Text>().font = Resources.Load<Font>("Art/fonts/Belwe-Bold");
                    Template_Text.GetComponent<Text>().color = Color.black;
                    Template_Text.GetComponent<Text>().lineSpacing = 1.5f;
                    Template_Text.GetComponent<RectTransform>().sizeDelta = new Vector2(((MBAQuestElement)obj).Width, ((MBAQuestElement)obj).Height);
                    JWInstantiateUnderParent_UI(Template_Text, Content, "Text", ((MBAQuestElement_Text)obj).Content, true, CalculateLocalLocation(((MBAQuestElement)obj).Relative, ((MBAQuestElement)obj).LocalPosition, Content), Vector3.one, Vector3.zero);
                    break;
                case "MBAQuestElement_Camp":
                    MBAQuestElement_Camp camp = (MBAQuestElement_Camp)obj;
                    GameObject newPanelCamp_DDM = JWInstantiateUnderParent_UI(Template_DDM, Content, "Panel", "", true, CalculateLocalLocation(camp.Relative, camp.LocalPosition, Content), Vector3.one, Vector3.zero);
                    newPanelCamp_DDM.GetComponent<MBAPanelDDM>().Side = "camp";
                    newPanelCamp_DDM.name = camp.Name;
                    newPanelCamp_DDM.transform.FindChild("Text_Name").GetComponent<Text>().text = camp.Name;
                    float campWidth = camp.Width > 0 ? camp.Width : newPanelCamp_DDM.GetComponent<RectTransform>().rect.width;
                    float campHeight = camp.Height > 0 ? camp.Height : newPanelCamp_DDM.GetComponent<RectTransform>().rect.height;
                    newPanelCamp_DDM.GetComponent<RectTransform>().sizeDelta = new Vector2(campWidth, campHeight);
                    newPanelCamp_DDM.transform.FindChild("Content").GetComponent<RectTransform>().sizeDelta = new Vector2(campWidth, campHeight);

                    MBAPanelDDM newMBAPanelDDM = newPanelCamp_DDM.GetComponent<MBAPanelDDM>();
                    foreach (MBAQuestElement_CampItem item in camp.Items)
                    {
                        items.Add(item);
                        newMBAPanelDDM.ExpectedItems.Add(item);
                    }
                    break;
                case "MBAQuestElement_Table":                    
                    MBAQuestElement_Table table = (MBAQuestElement_Table)obj;
                    GameObject newTable = JWInstantiateUnderParent_UI(Template_Table, Content, "Panel", "", true, CalculateLocalLocation(table.Relative, table.LocalPosition, Content), Vector3.one, Vector3.zero);
                    float tableWidth = table.Width > 0 ? table.Width : newTable.GetComponent<RectTransform>().rect.width;
                    float tableHeight = table.Height > 0 ? table.Height : newTable.GetComponent<RectTransform>().rect.height;
                    newTable.GetComponent<RectTransform>().sizeDelta = new Vector2(tableWidth, tableHeight);
                    //Template_Table
                    newTable.GetComponent<JWPanel_Table>().UpdateContent(table);
                    break;
                case "MBAQuestElement_Chart":
                    MBAQuestElement_Chart chart = (MBAQuestElement_Chart)obj;
                    GameObject newChart = JWInstantiateUnderParent_UI(Template_Chart, Content, "Panel", "", true, CalculateLocalLocation(chart.Relative, chart.LocalPosition, Content), Vector3.one, Vector3.zero);
                    newChart.GetComponent<JWPanel_Chart>().UpdateChart(chart);
                    break;
                case "MBAQuestElement_Solution":
                    solution = (MBAQuestElement_Solution)obj;
                    break;
            }
            objNumber++;
        }

        //UpdateChoices
        UpdateElement_Choices(choices_Temp, Template_Toggle);

        //instantiate stock panel
        if (stocks.Count > 0)
        {
            ToggleDowntownAndSkipButtons(false);
            GameObject newPanel_Stock = JWInstantiateUnderParent(Template_Stock, Content, CalculateLocalLocation(problem.Relative, new Vector2(0.5f, -200), Content));
            int i = 0;
            foreach (MBAQuestElement_Stock stock in stocks)
            {
                GameObject newStockRecord = JWInstantiateUnderParent(Template_StockRecord, newPanel_Stock, new Vector3(0, 100 - 40 * i, 0));
                newStockRecord.transform.FindChild("Text_StockName").GetComponent<Text>().text = stock.Name;
                newStockRecord.transform.FindChild("Text_OldPrice").GetComponent<Text>().text = (stock.Oldprice).ToString();
                newStockRecord.transform.FindChild("Text_NewPrice").GetComponent<Text>().text = (stock.Newprice).ToString();
                newStockRecord.GetComponent<MBAUIPanel_StockRecord>().Why = stock.Why;
                newStockRecord.GetComponent<MBAUIPanel_StockRecord>().Price_Old = stock.Oldprice;
                newStockRecord.GetComponent<MBAUIPanel_StockRecord>().Price_New = stock.Newprice;

                Text text_NewPrice = newStockRecord.transform.FindChild("Text_NewPrice").GetComponent<Text>();
                text_NewPrice.color = new Color(text_NewPrice.color.r, text_NewPrice.color.g, text_NewPrice.color.b, 0);
                Button button_Why = newStockRecord.transform.FindChild("Text_NewPrice").GetComponentInChildren<Button>();
                if (button_Why != null)
                {
                    button_Why.interactable = false;
                    Image image_ButtonWhy = button_Why.GetComponent<Image>();
                    image_ButtonWhy.color = new Color(image_ButtonWhy.color.r, image_ButtonWhy.color.g, image_ButtonWhy.color.b, 0);
                }

                newStockRecord.GetComponent<MBAUIPanel_StockRecord>().ShouldBuy = stock.ShouldBuy;

                i++;
            }
            

        }

        //Instantiate DDM Pool
        if (items.Count > 0)
        {
            GameObject newPanelPool_DDM = JWInstantiateUnderParent(Template_DDM, Content, CalculateLocalLocation(problem.Relative, problem.LocalPosition, Content));
            newPanelPool_DDM.GetComponent<MBAPanelDDM>().Side = "pool";
            newPanelPool_DDM.name = problem.Name;
            newPanelPool_DDM.transform.FindChild("Text_Name").GetComponent<Text>().text = problem.Name;
            newPanelPool_DDM.GetComponent<RectTransform>().sizeDelta = new Vector2(problem.Width_Pool, 0);

            int itemsCount = items.Count;
            for (int i = 0; i < itemsCount; i++)
            {
                yield return new WaitForSeconds(0.1f);
                int rand = UnityEngine.Random.Range(0, items.Count);
                MBAQuestElement_CampItem item = items[rand];

                GameObject newItem = JWInstantiateUnderParent(Template_DDMItem, Content, Vector3.zero);
                newItem.GetComponentInChildren<Text>().text = item.Text;
                if (item.Img == "")
                {
                    Color itemColor = newItem.GetComponent<Image>().color;
                    newItem.GetComponent<Image>().color = new Color(itemColor.r, itemColor.g, itemColor.b, 0);
                }
                else
                {
                    Color itemColor = newItem.GetComponent<Image>().color;
                    newItem.GetComponent<Image>().color = new Color(itemColor.r, itemColor.g, itemColor.b, 1);
                    newItem.GetComponent<Image>().sprite = LoadSprite(item.Img);
                    newItem.GetComponent<Image>().preserveAspect = true;
                }
                Transform content_Pool = newPanelPool_DDM.transform.FindChild("Content");
                content_Pool.GetComponent<RectTransform>().sizeDelta = new Vector2(problem.Width_Pool, 0);
                newItem.transform.SetParent(content_Pool);

                items.RemoveAt(rand);
            }
        }
        Content.GetComponent<JWUI>().AutoSize(Content.GetComponent<RectTransform>().rect.width, problem.Height, 0, 100);
    }

    void UpdateElement_Choices(MBAQuestElement_Choices choices, GameObject Template_Toggle)
    {
        int choicesCount = choices.Content.Count;
        for (int i = 0; i < choicesCount; i++)
        {
            int rand = UnityEngine.Random.Range(0, choices.Content.Count);
            MBAQuestElement_Choice choice = choices.Content[rand];
            float choiceWidth = choice.Width > 0 ? choice.Width : Template_Toggle.GetComponent<RectTransform>().rect.width;
            float choiceHeight = choice.Height > 0 ? choice.Height : Template_Toggle.GetComponent<RectTransform>().rect.height;
            Vector3 currLocalPosition = CalculateLocalLocation(choices.Relative, choices.LocalPosition, Content) + new Vector2(0, -i * choiceHeight + 10);
            GameObject newToggle = JWInstantiateUnderParent_UI(Template_Toggle, Content, "", "", true, currLocalPosition, Template_Toggle.transform.localScale, Template_Toggle.transform.localEulerAngles);
            newToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(choiceWidth, choiceHeight);
            newToggle.GetComponent<Toggle>().isOn = false;
            newToggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { UpdateSubmitAvailability(); });
            newToggle.name = choice.Text;
            newToggle.GetComponentInChildren<Text>().text = newToggle.name;
            newToggle.GetComponent<MBAMCOption>().isRight = choice.isRight;

            choices.Content.RemoveAt(rand);
        }
    }

    public void UpdateSubmitAvailability()
    {
        if (submitted)
            return;

        //Check inputfields
        bool inputFieldsFinished = true;
        InputField[] inputfields = FindObjectsOfType<InputField>();
        foreach (InputField inputfield in inputfields)
        {
            if (inputfield.GetComponent<MBAProblem_Blank>().isTemplate)
                continue;
            if (inputfield.text == "")
                inputFieldsFinished = false;
        }

        //Check stocks
        bool StockFinished = true;
        MBAUIPanel_StockRecord[] stockrecords = FindObjectsOfType<MBAUIPanel_StockRecord>();
        foreach (MBAUIPanel_StockRecord stockrecord in stockrecords)
        {
            if (!stockrecord.Decided)
            {
                StockFinished = false;
                break;
            }
        }
        StockFinished = true; //StockFinished is forced to be always true

        //Check Dropdowns
        bool dropdownsFinished = true;
        Dropdown[] dropdowns = FindObjectsOfType<Dropdown>();
        foreach (Dropdown dropdown in dropdowns)
        {
            if (dropdown.GetComponent<MBAProblem_Dropdown>().isTemplate)
                continue;
            if (dropdown.captionText.text == "")
                dropdownsFinished = false;
        }

        //Check toggles
        bool togglesFinished = false;
        Toggle[] toggles = FindObjectsOfType<Toggle>();
        if (toggles.Length <= 0)
            togglesFinished = true;
        else
        {
            foreach (Toggle toggle in toggles)
            {
                if (toggle.isOn)
                {
                    togglesFinished = true;
                    break;
                }
            }
        }

        //Check DDM (Any camp has item)
        bool DDMFinished = false;
        MBAPanelDDM[] panelsDDM = FindObjectsOfType<MBAPanelDDM>();
        if (panelsDDM.Length <= 0)
            DDMFinished = true;
        else
        {
            foreach (MBAPanelDDM panelDDM in panelsDDM)
            {
                if (panelDDM.Side == "camp" && panelDDM.transform.FindChild("Content").childCount > 0)
                {
                    DDMFinished = true;
                    break;
                }
            }
        }

        //Update all finish
        bool allFinished = inputFieldsFinished && togglesFinished && DDMFinished && dropdownsFinished && StockFinished;
        Button Button_Submit = transform.FindChild("MBAUIButton_ProblemSubmit").GetComponent<Button>();

        if (!allFinished){
            Button_Submit.interactable = false;
            Button_Submit.GetComponentInChildren<Text>().text = "";
        }
        else{
            Button_Submit.interactable = true;
            if (stockrecords.Length > 0)
                Button_Submit.GetComponentInChildren<Text>().text = "Decision\nTaken";
            else
                Button_Submit.GetComponentInChildren<Text>().text = "Submit";
        }
    }

    void TickCross(bool isTick, GameObject askObject, float imageLength, float textLength, float intervalLength, string rightanswer)
    {
        GameObject Template_Image = Resources.Load<GameObject>("Prefab/UI/Image");
        GameObject Template_Text = Resources.Load<GameObject>("Prefab/UI/Text");
        Vector3 tickcrossPosition = new Vector3(askObject.GetComponent<RectTransform>().rect.width / 2.0f + imageLength / 2.0f + intervalLength, 0, 0);
        GameObject newImage_Tick = JWInstantiateUnderParent(Template_Image, askObject, tickcrossPosition);
        if (isTick)
            newImage_Tick.GetComponent<Image>().sprite = tick;
        else
            newImage_Tick.GetComponent<Image>().sprite = cross;
        newImage_Tick.GetComponent<RectTransform>().sizeDelta = new Vector2(imageLength, imageLength);

        GameObject newText = JWInstantiateUnderParent(Template_Text, askObject.gameObject, tickcrossPosition + new Vector3(textLength / 2.0f + imageLength / 2.0f + intervalLength, 0, 0));
        newText.GetComponent<RectTransform>().sizeDelta = new Vector2(textLength, 60);
        newText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        newText.GetComponent<Text>().lineSpacing = 2;
        newText.GetComponent<Text>().text = rightanswer;
    }

    void Reward(string type)
    {
        int moneyPara = 0;
        int prestigePara = 0;

        int moneyFactor = db.Settings.Rewards.Money.Factor;
        int prestigeFactor = db.Settings.Rewards.Prestige.Factor;

        switch (type)
        {
            case "blank":
                moneyPara = db.Settings.Rewards.Money.Blank;
                prestigePara = db.Settings.Rewards.Prestige.Blank;
                break;
            case "ddm":
                moneyPara = db.Settings.Rewards.Money.DDM;
                prestigePara = db.Settings.Rewards.Prestige.DDM;
                break;
            case "mc":
            case "dropdown":
                moneyPara = db.Settings.Rewards.Money.MC;
                prestigePara = db.Settings.Rewards.Prestige.MC;
                break;
            /*case "stock":
                moneyPara = settings.Rewards.Money.Stock;
                prestigePara = settings.Rewards.Prestige.Stock;
                break;*/
        }

        db.Inventory.Property.Money += moneyPara * moneyFactor;
        db.Inventory.Property.Prestige += prestigePara * prestigeFactor;

        //db.Inventory.UpdateXML();
        
        /*UpdateXmlAttribute("config/inventory.xml", "//property/@money", updatedMoney.ToString());
        UpdateXmlAttribute("config/inventory.xml", "//property/@prestige", updatedPrestige.ToString());
        */
    }

    void Reward_Attemped(string type)
    {
        //int moneyPara = 0;
        int prestigePara = 0;

        //int moneyFactor = settings.Rewards.Money.Factor;
        int prestigeFactor = db.Settings.Rewards.Prestige.Factor;

        switch (type)
        {
            case "blank":
                //moneyPara = db.Settings.Rewards.Money.Blank;
                prestigePara = db.Settings.Rewards.Prestige.Blank;
                break;
            case "ddm":
                //moneyPara = db.Settings.Rewards.Money.DDM;
                prestigePara = db.Settings.Rewards.Prestige.DDM;
                break;
            case "mc":
            case "dropdown":
                //moneyPara = db.Settings.Rewards.Money.MC;
                prestigePara = db.Settings.Rewards.Prestige.MC;
                break;
            /*case "stock":
                moneyPara = settings.Rewards.Money.Stock;
                prestigePara = settings.Rewards.Prestige.Stock;
                break;*/
        }

        //inventory.Property.Money += moneyPara * moneyFactor;
        db.Inventory.Property.Prestige_Attempted += prestigePara * prestigeFactor;

        //print(updatedMoney + " " + updatedPrestige);
        //Time punishment
        if (db.Inventory.TimeChanged(db.Settings.Property.RatioTimePresitge))
        {
            foreach (MBAInventory_Colony colony in db.Inventory.Colonies.Content)
            {
                string alertText = colony.TimePunishment_ReturnPunishText();
                if (alertText != "")
                {
                    GameObject MBAPanel_Alert = Resources.Load<GameObject>("Prefab/GameElement/MBAPanel_Alert");
                    GameObject newAlert = JWInstantiateUnderParent_UI(MBAPanel_Alert, FindObjectOfType<Canvas>().gameObject, true);
                    newAlert.GetComponentInChildren<Text>().text = alertText;
                }
            }
        }
        //inventory.UpdateXML();

        /*UpdateXmlAttribute("config/inventory.xml", "//property/@money", updatedMoney.ToString());
        UpdateXmlAttribute("config/inventory.xml", "//property/@prestige", updatedPrestige.ToString());
        */
    }

    void ShowRewardPanel(int money, int prestige)
    {
        //Show reward panel
        GameObject Panel_RewardAlert = Resources.Load<GameObject>("Prefab/GameElement/MBAPanel_Reward");
        GameObject newPanel = JWInstantiateUnderParent_UI(Panel_RewardAlert, FindObjectOfType<Canvas>().gameObject, false);
        string rewardText = "You have not earned anything in this exercise";
        if (money != 0)
        {
            rewardText = "You have earned:\nMoney:" + money;
            if (prestige != 0)
                rewardText += "\nPrestige:" + prestige;
        }else if (prestige != 0)
            rewardText = "You have earned:\nPrestige:" + prestige;

        newPanel.GetComponentInChildren<Text>().text = rewardText;
    }   

    public void Submit()
    {
        ToggleDowntownAndSkipButtons(true);
        //init
        float imageLength = 20;
        float textLength = 250;
        float intervalLength = 10;

        //For Done button
        if (submitted)
        {
            SubmitDone();
            return;
        }
        submitted = true;

        //Hide help panel
        if (FindObjectOfType<MBAProblem_Help>())
        {
            MBAProblem_Help help = FindObjectOfType<MBAProblem_Help>();
            if (help.Visable)
                help.ToggleVis();
        }

        //solution button
        if (solution != null)
        {
            GameObject Panel_Solution = Resources.Load<GameObject>("Prefab/Quest/MBAUIPanel_Solution");
            Panel_Solution.GetComponentInChildren<Text>().text = solution.Text;
            GameObject Button_Solution = Resources.Load<GameObject>("Prefab/Quest/MBAUIButton_Solution");
            JWInstantiateUnderParent_UI(Button_Solution, gameObject, false);
        }

        //check answers for blanks
        InputField[] inputfields = FindObjectsOfType<InputField>();
        foreach(InputField inputfield in inputfields)
        {
            if (inputfield.GetComponent<MBAProblem_Blank>().isTemplate)
                continue;
            inputfield.enabled = false;

            MBAProblem_Blank currBlank = inputfield.GetComponent<MBAProblem_Blank>();
            float answer_Float;
            string rightanswer = currBlank.RightAnswer;
            float rightanswer_Float;
            bool correctAnswer = false;
            if (currBlank.Margin.magnitude > 0 
                && Single.TryParse(inputfield.text, out answer_Float)
                && Single.TryParse(rightanswer, out rightanswer_Float))
            {
                if (rightanswer_Float - currBlank.Margin.x <= answer_Float && answer_Float <= rightanswer_Float + currBlank.Margin.y)
                    correctAnswer = true;
            }
            else if (inputfield.text == rightanswer)
                correctAnswer = true;

            string rightAnswerString = "";
            if (correctAnswer)
            {
                if (inputfield.text != rightanswer)
                    rightAnswerString = "However, the best answer is " + rightanswer;
                TickCross(true, inputfield.gameObject, imageLength, textLength, intervalLength, rightAnswerString);

                //reward
                Reward("blank");
            }
            else
            {
                if (currBlank.Margin.magnitude > 0 && Single.TryParse(rightanswer, out rightanswer_Float))
                {
                    if (currBlank.Margin.x == currBlank.Margin.y)
                        rightAnswerString = "Right answer is " + rightanswer;// + "±" + (currBlank.Margin.x).ToString() + ".";
                    else
                        rightAnswerString = "Right answer is [" + (rightanswer_Float - currBlank.Margin.x).ToString() + ", " + (rightanswer_Float + currBlank.Margin.y).ToString() + "].";
                }
                else
                    rightAnswerString = "Right answer is " + rightanswer;
                TickCross (false, inputfield.gameObject, imageLength, textLength, intervalLength, rightAnswerString);
            }
            Reward_Attemped("blank");
        }

        //check answers for toggles
        Toggle[] toggles = FindObjectsOfType<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            toggle.enabled = false;
            if (toggle.GetComponent<MBAMCOption>().isRight)
            {
                if (toggle.isOn)
                {
                    Reward_Attemped("mc");
                    Reward("mc");
                }
                TickCross(true, toggle.gameObject, imageLength, textLength, intervalLength, "");
            }
            else if (toggle.isOn)
            {
                TickCross(false, toggle.gameObject, imageLength, textLength, intervalLength, "");
                Reward_Attemped("mc");
            }
                
        }

        //check answers for stocks
        MBAUIPanel_StockRecord[] stockrecords = FindObjectsOfType<MBAUIPanel_StockRecord>();

        foreach (MBAUIPanel_StockRecord stockrecord in stockrecords)
        {

            //print(stockrecord.Quant * stockrecord.Price_New - stockrecord.Quant * stockrecord.Price_Old);
            db.Inventory.Property.Money += stockrecord.Quant * stockrecord.Price_New;
            //inventory.UpdateXML();

            if (stockrecord.ShouldBuy == stockrecord.chosetoBuy)
            {
                //TickCross(true, stockrecord.gameObject, imageLength, textLength, 330, "");
                //Reward("stock");
            }
            else
            {
                //TickCross(false, stockrecord.gameObject, imageLength, textLength, 330, "");
            }
            Text Text_NewPrice = stockrecord.transform.FindChild("Text_NewPrice").GetComponent<Text>();
            Text_NewPrice.color = new Color(Text_NewPrice.color.r, Text_NewPrice.color.g, Text_NewPrice.color.b, 1);
            //Text_NewPrice.text = "$" + stockrecord.Price_New;

            Button button_Why = stockrecord.transform.FindChild("Text_NewPrice").GetComponentInChildren<Button>();
            if (button_Why != null)
            {
                button_Why.interactable = true;
                Image image_ButtonWhy = button_Why.GetComponent<Image>();
                image_ButtonWhy.color = new Color(image_ButtonWhy.color.r, image_ButtonWhy.color.g, image_ButtonWhy.color.b, 1);
            }

            Reward_Attemped("stock");
        }


        //Check answers for Dropdown
        Dropdown[] dropdowns = FindObjectsOfType<Dropdown>();
        foreach (Dropdown dropdown in dropdowns)
        {
            dropdown.enabled = false;
            if (dropdown.captionText.text == dropdown.GetComponent<MBAProblem_Dropdown>().RightAnswer)
            {
                Reward("dropdown");
                TickCross(true, dropdown.gameObject, imageLength, textLength, intervalLength, "");
            }
            else
                TickCross(true, dropdown.gameObject, imageLength, textLength, intervalLength, "");

            Reward_Attemped("dropdown");
        }

        //Check answers for DDM
        MBAPanelDDM[] panels = FindObjectsOfType<MBAPanelDDM>();
        foreach (MBAPanelDDM panel in panels)
        {
            bool missingItem = false;
            bool unexpectedItem = false;

            if (panel.Side == "camp")
            {
                //missing item
                foreach (MBAQuestElement_CampItem item in panel.ExpectedItems)
                {
                    bool itemInCamp = false;
                    foreach (Transform child in panel.transform.FindChild("Content"))
                    {
                        if (child.GetComponentInChildren<Text>().text == item.Text)
                        {
                            itemInCamp = true;
                            break;
                        }
                    }
                    if (!itemInCamp)
                    {
                        missingItem = true;
                        break;
                    }
                }

                //unexpected item
                foreach (Transform child in panel.transform.FindChild("Content"))
                {
                    bool childFoundInExpectedItems = false;
                    foreach (MBAQuestElement_CampItem item in panel.ExpectedItems)
                    {
                        if (child.GetComponentInChildren<Text>().text == item.Text)
                        {
                            childFoundInExpectedItems = true;
                            break;
                        }
                    }
                    if (!childFoundInExpectedItems)
                        unexpectedItem = true;
                }
                bool panelAnswerRight = !missingItem && !unexpectedItem;
                if (panelAnswerRight)
                {
                    TickCross(true, panel.gameObject, imageLength, textLength, intervalLength, "");
                    Reward("ddm");
                }
                else
                    TickCross(false, panel.gameObject, imageLength, textLength, intervalLength, "");

                Reward_Attemped("ddm");

            }
        }

        //finish curr content
        //StartCoroutine(SubmitEnd());
        int earnedMoney = db.Inventory.Property.Money - startMoney;
        int earnedPrestige = db.Inventory.Property.Prestige - startPrestige;
        ShowRewardPanel(earnedMoney, earnedPrestige);

    }

    void SubmitDone()
    {
        FindObjectOfType<MBABuildingInside>().FinishCurrElement();
        Destroy(gameObject);
    }

    /*IEnumerator SubmitEnd()
    {
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        FindObjectOfType<MBABuildingInside>().FinishCurrElement();
        Destroy(gameObject);
    }*/

    public void MoveObject(Transform tf, Vector3 offset)
    {
        JWEffect effect = tf.GetComponent<JWEffect>();
        effect.destinationOffset = offset;
    }

    public void ToggleMove()
    {
        Transform problem = transform.FindChild("Problem");
        Transform help = transform.FindChild("Help");
        if (help.GetComponent<JWEffect>().destinationOffset.magnitude != 0
            || problem.GetComponent<JWEffect>().destinationOffset.magnitude != 0)
            return;

        Vector3 problemOffset = new Vector3(-120, 0, 0);
        Vector3 helpOffset = new Vector3(210, -42, 0);

        if (!showingHelp)
        {
            MoveObject(problem, problemOffset);
            MoveObject(help, helpOffset);
            showingHelp = true;
        }
        else
        {
            MoveObject(problem, -problemOffset);
            MoveObject(help, -helpOffset);
            showingHelp = false;
        }
    }
}
