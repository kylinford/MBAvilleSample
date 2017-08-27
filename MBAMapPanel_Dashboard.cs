using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class MBAMapPanel_Dashboard : JWMonoBehaviour {
    GameObject Button_Quest;
    MBADataBuffer db;

	// Use this for initialization
	void Start () {
        db = Resources.Load<MBADataBuffer>("Prefab/MBADataBuffer");
        if (db.allQuests.Count == 0)
            db.InitQuests();
        Button_Quest = Resources.Load<GameObject>("Prefab/GameElement/MBAButton_Quest");
        //db.issuedQuests.Clear();
        UpdateList_Category();

    }

    // Update is called once per frame
    void Update () {
	
	}

    void InstantiateQuestButton(MBAQuest quest, int num)
    {
        float yspace = 10;
        GameObject Content = transform.FindChild("ScrollView_QuestButtons").FindChild("Viewport").FindChild("Content").gameObject;
        Content.GetComponent<RectTransform>().sizeDelta = new Vector2(Content.GetComponent<RectTransform>().sizeDelta.x, num * (83 + 15) + 100);
        GameObject newButton = JWInstantiateUnderParent_UI(Button_Quest, Content, false, new Vector3(Button_Quest.transform.localPosition.x, Button_Quest.transform.localPosition.y - num* (Button_Quest.GetComponent<RectTransform>().rect.height + yspace), 0));
        GameObject reddot = newButton.transform.FindChild("Image_RedDot").gameObject;
        if (!db.issuedQuests.Contains(quest.ID))
            reddot.SetActive(false);
        Text Text_Finished = newButton.transform.FindChild("Text_Finished").GetComponent<Text>();
        if (db.finishedQuests.Contains(quest.ID))
        {
            newButton.GetComponent<Image>().color = newButton.GetComponent<Image>().color * 0.9f;
            Text_Finished.text = "Finished";
        }

        MBASettings_Subject subject = db.Settings.GetSubjectByID(quest.Subject);
        string subjectName = subject == null ? "Subject Name N/A" : subject.Name;
        newButton.transform.FindChild("Text_Intro").GetComponent<Text>().text = "<b>" + quest.Name +  "</b>\n" + subjectName + " (ID: " + quest.ID + ")\n<color=grey>" + quest.Outcome + "</color> ";
        newButton.GetComponent<Button>().onClick.AddListener(delegate {
            db.currQuest = quest;
            db.currBuilding = quest.Building;
            InistantiateOutcomePanel(quest.Outcome);
            //SceneManager.LoadScene("BuildingInside");//building inside
        });
        
    }

    void InistantiateOutcomePanel(string outcome)
    {
        GameObject MBAPanel_Outcome = Resources.Load<GameObject>("Prefab/GameElement/MBAPanel_Outcome");
        GameObject newOutcomePanel = JWInstantiateUnderParent(MBAPanel_Outcome, FindObjectOfType<Canvas>().gameObject, Vector3.zero);
        newOutcomePanel.transform.FindChild("Text").GetComponent<Text>().text = outcome;
        Transform Button_Start = newOutcomePanel.transform.FindChild("Button_Start");
        Transform Button_Skip = newOutcomePanel.transform.FindChild("Button_Skip");

        
        if (!db.issuedQuests.Contains(db.currQuest.ID))
        {
            Button_Start.GetComponentInChildren<Text>().text = "To be unlocked";
            Button_Start.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            Vector3 startPosition = Button_Start.transform.localPosition;
            Button_Start.transform.localPosition = new Vector3(0, startPosition.y, startPosition.z);
            Button_Skip.gameObject.SetActive(false);
            Button_Start.GetComponent<Button>().enabled = false;
        }
        if (db.finishedQuests.Contains(db.currQuest.ID))
        {
            Button_Start.gameObject.SetActive(false);
            Button_Skip.gameObject.SetActive(false);
        }

        //Demo limit
        string maxQuestID = "16.1";
        if ((Single.Parse(db.currQuest.ID) > Single.Parse(maxQuestID) 
            && (int)Single.Parse(db.currQuest.ID) != 25))
        {
            if (db.UnlockedAll)
                return;
            Button_Start.GetComponentInChildren<Text>().text = "Unavailable in demo mode";
            Button_Start.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            Vector3 startPosition = Button_Start.transform.localPosition;
            Button_Start.transform.localPosition = new Vector3(0, startPosition.y, startPosition.z);
            Button_Start.GetComponent<RectTransform>().sizeDelta = new Vector2(Button_Start.GetComponent<RectTransform>().rect.width * 1.5f, Button_Start.GetComponent<RectTransform>().rect.height);
            Button_Skip.gameObject.SetActive(false);
            Button_Start.GetComponent<Button>().enabled = false;
        }
    }

    public void UpdateList_Quest(string listname)
    {
        MBAMapPanel_DashboardQuestButton[] buttons = FindObjectsOfType<MBAMapPanel_DashboardQuestButton>();
        foreach (MBAMapPanel_DashboardQuestButton button in buttons)
            Destroy(button.gameObject);
      
        string category = transform.FindChild("Panel_Category").FindChild("Dropdown_Category").GetComponent<Dropdown>().captionText.text;
        int questCount = 0;
        switch (category)
        {
            case "Subjects":
                foreach (MBAQuest quest in db.Quests)
                {
                    if (quest.Subject == listname)
                    {
                        //print(quest.Subject);
                        InstantiateQuestButton(quest, questCount);
                        questCount++;
                    }
                }
                break;
            case "Contacts":
                foreach (string questid in db.allQuests)
                {
                    MBAQuest quest = new MBAQuest(questid);
                    if (quest.Building == listname)
                    {
                        InstantiateQuestButton(quest, questCount);
                        questCount++;
                    }
                }
                break;
        }
        
    }

    public void UpdateList_Category()
    {
        GameObject Panel_CategoryList = transform.FindChild("Panel_Category").FindChild("Scroll View").FindChild("Viewport")
            .FindChild("Content").gameObject;
        GameObject ButtonTemplate = Resources.Load<GameObject>("Prefab/GameElement/MBAButton_Subject");
        string category = transform.FindChild("Panel_Category").FindChild("Dropdown_Category").GetComponent<Dropdown>().captionText.text;
        foreach(Transform child in Panel_CategoryList.transform)
            Destroy(child.gameObject);
        switch (category)
        {
            case "Contacts":
                List<string> buildings = new List<string>();
                foreach (string questid in db.allQuests)
                {
                    MBAQuest quest = new MBAQuest(questid);
                    if (!buildings.Contains(quest.Building))
                        buildings.Add(quest.Building);
                }
                foreach (string building in buildings)
                {
                    GameObject newButton = JWInstantiateUnderParent_UI(ButtonTemplate, Panel_CategoryList, false);
                    string currBuilding = building;
                    newButton.GetComponent<Button>().onClick.AddListener(delegate { UpdateList_Quest(currBuilding); });
                    newButton.GetComponentInChildren<Text>().text = db.Settings.GetBuildingNameByID(building);
                }
                break;
            case "Subjects":
                List<string> subjects = new List<string>();
                foreach (string questid in db.allQuests)
                {
                    MBAQuest quest = new MBAQuest(questid);
                    if (!subjects.Contains(quest.Subject))
                        subjects.Add(quest.Subject);
                }

                List<string> issuedSubjects = new List<string>();
                foreach (string issuedQuestID in db.issuedQuests)
                {
                    MBAQuest quest = new MBAQuest(issuedQuestID);
                    if(!issuedSubjects.Contains(quest.Subject))
                        issuedSubjects.Add(quest.Subject);
                }

                foreach (string issuedSubject in issuedSubjects)
                    InstantiateCategoryButton_Subject(issuedSubject);

                foreach (string subject in subjects)
                {
                    if (!issuedSubjects.Contains(subject))
                        InstantiateCategoryButton_Subject(subject);
                }
                break;
        }
        //Panel_CategoryList.GetComponentInChildren<Button>().onClick.Invoke();
        StartCoroutine(InvokeFirstChildButton(Panel_CategoryList));
    }

    void InstantiateCategoryButton_Subject(string subject)
    {
        int currAll = db.GetQuestCount_All("subject", subject);
        int currFinished = db.GetQuestCount_Finished("subject", subject);
        GameObject Panel_CategoryList = transform.FindChild("Panel_Category").FindChild("Scroll View").FindChild("Viewport")
            .FindChild("Content").gameObject;
        GameObject ButtonTemplate = Resources.Load<GameObject>("Prefab/GameElement/MBAButton_Subject");
        GameObject newButton = JWInstantiateUnderParent_UI(ButtonTemplate, Panel_CategoryList, false);
        GameObject reddot = newButton.transform.FindChild("Image_RedDot").gameObject;
        reddot.SetActive(false);
        
        GameObject Text_Percentage = newButton.transform.FindChild("Text_Percentage").gameObject;
        Text_Percentage.GetComponent<Text>().text = currFinished + "/" + currAll;
        Slider slider = newButton.GetComponentInChildren<Slider>();
        slider.value = (float)currFinished / (float)currAll;

        foreach (string questID in db.issuedQuests)
        {
            MBAQuest currQuest = new MBAQuest(questID);
            if (currQuest.Subject == subject)
            {
                reddot.SetActive(true);
                reddot.GetComponentInChildren<Text>().text = db.GetQuestCount_Issued("subject", subject).ToString();
                break;
            }
        }
        string currSubject = subject;
        newButton.GetComponent<Button>().onClick.AddListener(delegate {
            UpdateList_Quest(currSubject);
            //ShowSubjectIntro(settings.GetSubjectByID(currSubject));
        });
        newButton.GetComponentInChildren<Text>().text = db.Settings.GetSubjectNameByID(subject);
    }

    void ShowSubjectIntro(MBASettings_Subject subject)
    {
        GameObject Panel_Alert_GotIt = Resources.Load<GameObject>("Prefab/GameElement/MBAPanel_Alert_GotIt");
        string alertText = subject.Intro;
        GameObject newAlert = Alert(Panel_Alert_GotIt, alertText, false);
        newAlert.GetComponentInChildren<Text>().fontSize = subject.Font_Size_Intro;
    }

    IEnumerator InvokeFirstChildButton(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        go.GetComponentInChildren<Button>().onClick.Invoke();
    }
}
