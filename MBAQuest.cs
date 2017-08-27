using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

public class MBAQuest:MBAQuestElement
{
    public bool active = false;
    //MBAQuest_Help help;
    List<string> children = new List<string>();
    ArrayList content = new ArrayList();//Dialogues, problems
    string id;
    string building;
    string subject;
    string outcome;

    //public MBAQuest_Help Help { get { return help; } set { help = value; } }
    public List<string> Children { get { return children; } set { children = value; } }
    public ArrayList Elements { get { return content; } set { content = value; } }
    public string ID { get { return id; } set { id = value; } }
    public string Building { get { return building; } set { building = value; } }
    public string Subject { get { return subject; } set { subject = value; } }
    public string Outcome { get { return outcome; } set { outcome = value; } }

    IEnumerator CreateXmlReader(string url, string XmlData)
    {
        WWW www = new WWW(url);
        //yield return new WaitForSeconds(2);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error + " " + url);
        //StreamReader streamReader = new StreamReader(url);
        //reader = XmlReader.Create(streamReader);
        XmlData = www.text;// DataCollection.DeSerialize(result);
    }

    public MBAQuest(string questID)//load from local
    {
        string xmlData="";

		if (MBAville.MBAMonoBehaviour.ShouldLoadSource())
            xmlData = (Resources.Load("Content/QuestLib/" + questID + "/quest") as TextAsset).text;
        else
            xmlData = File.ReadAllText("QuestLib/" + questID + "/quest.xml");

        //xmlData = www.text;        
        if (xmlData == null)
            return;

        XmlReader reader = XmlReader.Create(new StringReader(xmlData));
        bool readerValid = true;
        if (reader == null)
        {
            Debug.Log("no data");
            readerValid = false;
        }

        if (readerValid)
        {
            //Debug.Log("3");
            reader.ReadStartElement();
            //Debug.Log("3.5");

            //Content
            while (reader.Read())
            {
                //Debug.Log("4");
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        LoadXmlElement(reader);
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.EndElement:
                    default:
                        break;
                }
            }
            //Debug.Log("5");
        }
    }

    /*public MBAQuest(string questID)//load from server
    {

        //property = new MBAQuestElement_Property();
        //Property.ID = questID;

        //Create xmlreaqder by resource loading
        Debug.Log("MBAQuest begin");
        XmlReader reader;
        bool readerValid = true;
        if (Application.platform == RuntimePlatform.WindowsEditor && false)
        {
            TextAsset xmlData = Resources.Load("xml/QuestLib/" + questID + "/quest") as TextAsset;
            if (xmlData == null)
                readerValid = false;
            reader = XmlReader.Create(new StringReader(xmlData.text));//
        }
        else
        {
            Debug.Log("reading xml online");
            //Create xmlreader by external url
            string xmlURL = "http://www.mbaville.com/Data/xml/QuestLib/" + questID + "/quest.xml";
            Debug.Log("1");
            //xmlURL = "xml/QuestLib/" + questID + "/quest.xml";
            //reader = new XmlTextReader(xmlURL);
            //StartCoroutine(CreateXmlReader(xmlURL, xmlData));

            WWW www = new WWW(xmlURL);
            if (!string.IsNullOrEmpty(www.error) || www.isDone)
                Debug.Log(www.error + " " + xmlURL);
            //StreamReader streamReader = new StreamReader(url);
            //reader = XmlReader.Create(streamReader);
            string xmlData = www.text;// DataCollection.DeSerialize(result);

            Debug.Log(xmlData);
            //reader = XmlReader.Create(xmlURL);
            Debug.Log("2");
            //StreamReader streamReader = new StreamReader();
            // Create the XmlReader object.
            reader = XmlReader.Create(new StringReader(xmlData));
            Debug.Log("3");
            //reader = XmlReader.Create(textReader);
            Debug.Log("4");
            if (reader == null)
            {
                Debug.Log("no data");
                readerValid = false;
            }
            Debug.Log("5");

        }
        if (readerValid)
        {
            reader.ReadStartElement();
            Debug.Log("6");

            //Content
            while (reader.Read())
            {
                Debug.Log("reading content");

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        LoadXmlElement(reader);
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.EndElement:
                    default:
                        break;
                }
            }
            //print();
            Debug.Log("MBAQuest finished");
        }
        

    }
    */

    void LoadXmlElement(XmlReader reader)
    {
        XmlReader readerSubtree = reader.ReadSubtree();
        if (readerSubtree == null)
            return;
        switch (reader.Name)
        {
            case "text":
                content.Add(new MBAQuestElement_Text(reader));
                break;
            case "dialogue":
                content.Add(new MBAQuestElement_Dialogue(reader));
                break;
            case "problem":
                content.Add(new MBAQuestElement_Problem(reader));
                break;
            case "property":
                SetProperty(new MBAQuestElement_Property(reader));
                break;
            case "children":
                SetChildren(reader);
                break;
            default:
                break;
        }
    }

    public void print()
    {
        foreach (object obj in content)
        {
            Type objType = obj.GetType();
            switch (objType.Name)
            {
                case "Dialogue":
                    ((MBAQuestElement_Dialogue)obj).print();
                    break;
                case "Problem":
                    ((MBAQuestElement_Problem)obj).print();
                    break;
                case "Property":
                    ((MBAQuestElement_Property)obj).print();
                    break;
                default: break;
            }
        }
    }

    void SetChildren(XmlReader reader)
    {
        XmlReader readerSubtree = reader.ReadSubtree();

        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                default:
                    break;
            }
        }
        //Customize label
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "child":
                            XmlReader readerSubtreeSubtree = readerSubtree.ReadSubtree();
                            while (readerSubtreeSubtree.Read())
                            {
                                switch (readerSubtreeSubtree.NodeType)
                                {
                                    case XmlNodeType.Text:
                                        children.Add(readerSubtreeSubtree.Value);
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
    }
    void SetProperty(MBAQuestElement_Property currProperty)
    {
        id = currProperty.ID;
        elementName = currProperty.Name;
        height = currProperty.Height;
        width = currProperty.Width;
        building = currProperty.Building;
        subject = currProperty.Subject;
        outcome = currProperty.Outcome;
    }
}

public class MBAQuestElement
{
    protected string elementName = "Untitled";
    protected Vector2 localPosition = Vector2.zero;
    protected string relative = "x";
    protected float width = 0;
    protected float height = 0;
    protected float readtime;

    public float ReadTime { get { return readtime; } set { readtime = value; } }
    public string Name { get { return elementName; } set { elementName = value; } }
    public string Relative { get { return relative; } set { relative = value; } }
    public Vector2 LocalPosition { get { return localPosition; } set { localPosition = value; } }
    public float Width { get { return width; } set { width = value; } }
    public float Height { get { return height; } set { height = value; } }
}

public class MBAQuestElement_Problem:MBAQuestElement
{
    string type;
    ArrayList content;
    float width_pool = 300;

    public string Type { get { return type; } set { type = value; } }
    public ArrayList Content { get { return content; } set { content = value; } }
    public float Width_Pool { get { return width_pool; } }

    public MBAQuestElement_Problem(XmlReader reader)
    {
        XmlReader readerSubtree = reader.ReadSubtree();
        content = new ArrayList();
        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "type":
                    type = reader.Value;
                    break;
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    Single.TryParse(reader.Value, out localPosition.x);
                    break;
                case "y":
                    Single.TryParse(reader.Value, out localPosition.y);
                    break;
                case "width":
                    Single.TryParse(reader.Value, out width);
                    break;
                case "height":
                    Single.TryParse(reader.Value, out height);
                    break;
                case "width-pool":
                    Single.TryParse(reader.Value, out width_pool);
                    break;

                default:
                    break;
            }
        }

        //Customize object
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                /*case XmlNodeType.Text:
                    content.Add(reader.Value);
                    break;*/
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "blank":
                            content.Add(new MBAQuestElement_Blank(reader));
                            break;
                        case "choices":
                            content.Add(new MBAQuestElement_Choices(reader));
                            break;
                        case "choice":
                            content.Add(new MBAQuestElement_Choice(reader));
                            break;
                        case "camp":
                            content.Add(new MBAQuestElement_Camp(reader));
                            break;
                        case "img":
                            content.Add(new MBAQuestElement_Image(reader));
                            break;
                        case "text":
                            content.Add(new MBAQuestElement_Text(reader));
                            break;
                        case "table":
                            content.Add(new MBAQuestElement_Table(reader));
                            break;
                        case "help":
                            content.Add(new MBAQuestElement_Help(reader));
                            break;
                        case "chart":
                            content.Add(new MBAQuestElement_Chart(reader));
                            break;
                        case "solution":
                            content.Add(new MBAQuestElement_Solution(reader));
                            break;
                        case "stock":
                            content.Add(new MBAQuestElement_Stock(reader));
                            break;

                    }
                    break;
            }
        }
    }

    public void print()
    {
        string output = Name;
        Debug.Log(output);
    }

}

public class MBAQuestElement_Stock: MBAQuestElement
{
    int oldprice = 0;
    int newprice = 0;
    bool shouldBuy = true;
    string why = "";
    public int Oldprice { get { return oldprice; } }
    public int Newprice { get { return newprice; } }
    public bool ShouldBuy { get { return shouldBuy; } }
    public string Why { get { return why; } }

    public MBAQuestElement_Stock(XmlReader reader)
    {
        elementName = "N/A";
        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "oldprice":
                    oldprice = Int32.Parse(reader.Value);
                    break;
                case "newprice":
                    newprice = Int32.Parse(reader.Value);
                    break;
                case "shouldbuy":
                    if (reader.Value == "false")
                        shouldBuy = false;
                    break;
                case "why":
                    why = reader.Value;
                    break;
                default:
                    break;
            }
        }

    }
}

class MBAQuestElement_Solution : MBAQuestElement
{
    string text;
    ArrayList content;
    public string Text { get { return text; } set { text = value; } }
    public ArrayList Content { get { return content; } set { content = value; } }

    public MBAQuestElement_Solution(XmlReader reader)
    {
        XmlReader readerSubtree = reader.ReadSubtree();
        content = new ArrayList();
        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                default:
                    break;
            }
        }

        //Customize object
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    content.Add(reader.Value);
                    text = reader.Value;
                    break;
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "blank":
                            content.Add(new MBAQuestElement_Blank(reader));
                            break;
                        case "choice":
                            content.Add(new MBAQuestElement_Choice(reader));
                            break;
                        case "camp":
                            content.Add(new MBAQuestElement_Camp(reader));
                            break;
                        case "img":
                            content.Add(new MBAQuestElement_Image(reader));
                            break;
                        case "text":
                            content.Add(new MBAQuestElement_Text(reader));
                            break;
                        case "table":
                            content.Add(new MBAQuestElement_Table(reader));
                            break;
                    }
                    break;
            }
        }
    }

}

public class MBAQuestElement_Help: MBAQuestElement
{
    ArrayList content;
    public ArrayList Content { get { return content; } set { content = value; } }

    public MBAQuestElement_Help(XmlReader reader)
    {
        elementName = "CONCEPT";
        XmlReader readerSubtree = reader.ReadSubtree();
        content = new ArrayList();
        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                default:
                    break;
            }
        }

        //Customize object
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    content.Add(reader.Value);
                    break;
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "blank":
                            content.Add(new MBAQuestElement_Blank(reader));
                            break;
                        case "choice":
                            content.Add(new MBAQuestElement_Choice(reader));
                            break;
                        case "camp":
                            content.Add(new MBAQuestElement_Camp(reader));
                            break;
                        case "img":
                            content.Add(new MBAQuestElement_Image(reader));
                            break;
                        case "text":
                            content.Add(new MBAQuestElement_Text(reader));
                            break;
                        case "table":
                            content.Add(new MBAQuestElement_Table(reader));
                            break;
                    }
                    break;
            }
        }
    }

}

class MBAQuestElement_Text : MBAQuestElement
{
    string content;
    public string Content { get { return content; } set { content = value; } }

    public MBAQuestElement_Text(XmlReader reader) 
    {
        XmlReader readerSubtree = reader.ReadSubtree();
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    localPosition.x = Single.Parse(reader.Value);
                    break;
                case "y":
                    localPosition.y = Single.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                default:
                    break;
            }
        }

        width = width > 0 ? width : 160;
        height = height > 0 ? height : 30;

        //readerSubtree.Settings.IgnoreWhitespace = true;
        content = "";
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    content += readerSubtree.Value;
                    break;
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "blank":
                            //content.Add(new MBAQuestElement_Blank(reader));
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
    }
}

class MBAQuestElement_Dialogue: MBAQuestElement
{
    string src="";
    Vector2 position;
    string text;
    int fontsize = 30;

    public string Src { get { return src; } set { src = value; } }
    public Vector2 Position { get { return position; } set { position = value; } }
    public string Text { get { return text; } set { text = value; } }
    public int FontSize { get { return fontsize; } set { fontsize = value; } }
    public MBAQuestElement_Dialogue(XmlReader reader)
    {
        //Dialogue dialogue = new Dialogue();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "src":
                    src = reader.Value;
                    break;
                case "readtime":
                    Single.TryParse(reader.Value, out readtime);
                    break;
                case "font-size":
                    int.TryParse(reader.Value, out fontsize);
                    break;
                case "position":
                    switch (reader.Value)
                    {
                        case "top":
                            position = new Vector2(0, 0.7f); break;
                        default:
                            position = Vector2.zero; break;
                    }
                    break;
            }
        }

        //Customize object
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();
        //readerSubtree.Settings.IgnoreWhitespace = true;
        text = "";

        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    text += readerSubtree.Value;
                    break;
            }
        }
    }

    public MBAQuestElement_Dialogue(string newSrc, Vector2 newPosition, string newText, float newReadtime)
    {
        src = newSrc;
        position = newPosition;
        text = newText;
        readtime = newReadtime;
    }
    
    public void print()
    {
        string output = "[Dialogue]";
        output += src + ": " + text;
        Debug.Log(output);
    }
}

class MBAQuestElement_Image : MBAQuestElement
{
    string src;
    public string Src { get { return src; } set { src = value; } }

    public MBAQuestElement_Image(XmlReader reader) 
    {
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    localPosition.x = Single.Parse(reader.Value);
                    break;
                case "y":
                    localPosition.y = Single.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                case "src":
                    src = reader.Value;
                    break;
                default:
                    break;
            }
        }
    }
}

class MBAQuestElement_Camp : MBAQuestElement
{
    string constraint = "flexible";
    int constraintcount = 3;
    string alignment = "middle center";
    List<MBAQuestElement_CampItem> items = new List<MBAQuestElement_CampItem>();
    int maxChildren = 50;

    public List<MBAQuestElement_CampItem> Items { get { return items; } set { items = value; } }
    public int MaxChildren { get { return maxChildren; } set { maxChildren = value; } }

    public MBAQuestElement_Camp(XmlReader reader)
    {
        items = new List<MBAQuestElement_CampItem>();
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    base.localPosition.x = float.Parse(reader.Value);
                    break;
                case "y":
                    base.localPosition.y = float.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                case "constraint":
                    constraint = reader.Value;
                    break;
                case "constraintcount":
                    constraintcount = Int32.Parse(reader.Value);
                    break;
                case "alignment":
                    alignment = reader.Value;
                    break;
                case "maxchildren":
                    maxChildren = Int32.Parse(reader.Value);
                    break;
                default:
                    break;
            }
        }

        //Customize object
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "item":
                            MBAQuestElement_CampItem item = new MBAQuestElement_CampItem(reader, elementName);
                            items.Add(item);
                            break;
                    }
                    break;
            }
        }
    }

    void print()
    {
        Debug.Log(elementName + " " + "localPosition=(" + localPosition.x + ", " + localPosition.y + ")" + width.ToString() + " " + height.ToString() + " " + constraint
            + " " + constraintcount.ToString() + " " + alignment + " " + (items.Count).ToString());
    }
}

public class MBAQuestElement_CampItem
{
    string camp;
    string text;
    string img = "";

    public string Camp { get { return camp; } set { camp = value; } }
    public string Text { get { return text; } set { text = value; } }
    public string Img { get { return img; } set { img = value; } }

    public MBAQuestElement_CampItem(string textContent, string campName)
    {
        camp = campName;
        text = textContent;
    }
    public MBAQuestElement_CampItem(XmlReader reader, string campName)
    {
        camp = campName;

        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "img":
                    img = reader.Value;
                    break;
                default:
                    break;
            }
        }
            //readerSubtree.Settings.IgnoreWhitespace = true;
            text = "";
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    text = readerSubtree.Value;
                    break;
            }
        }
    }
}

public class MBAQuestElement_Blank : MBAQuestElement
{
    string rightAnswer;
    string type;
    Vector2 margin = Vector2.zero;//min and max (left and right)

    public string RightAnswer { get { return rightAnswer; }  }
    public string Type { get { return type; } set { type = value; } }
    public Vector2 Margin { get { return margin; } set { margin = value; } }

    public MBAQuestElement_Blank(XmlReader reader)
    {        
        //
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    localPosition.x = Single.Parse(reader.Value);
                    break;
                case "y":
                    localPosition.y = Single.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                case "rightanswer":
                    rightAnswer = reader.Value;
                    //Default margin 4%
                    float rightAnswer_Float;
                    if (Single.TryParse(rightAnswer, out rightAnswer_Float))
                    {
                        float defaultMarginPercentage = 0.04f;
                        float defaultMargin = rightAnswer_Float * defaultMarginPercentage;
                        margin = new Vector2(defaultMargin, defaultMargin);
                    }
                    break;
                case "type":
                    type = reader.Value;
                    break;
                case "margin":
                    float marginValue;
                    if (Single.TryParse(reader.Value, out marginValue))
                        margin = new Vector2(marginValue, marginValue);
                    break;
                case "margin-left":
                    float marginValue_Left;
                    if (Single.TryParse(reader.Value, out marginValue_Left))
                        margin = new Vector2(marginValue_Left, margin.y);
                    break;
                case "margin-right":
                    float marginValue_Right;
                    if (Single.TryParse(reader.Value, out marginValue_Right))
                        margin = new Vector2(margin.x, marginValue_Right);
                    break;
                case "margin-percentage":
                    float marginValue_FromPercentage;
                    float margin_Percentage;
                    if (Single.TryParse(reader.Value, out margin_Percentage)
                        && Single.TryParse(rightAnswer, out rightAnswer_Float))
                    {
                        marginValue_FromPercentage = rightAnswer_Float * margin_Percentage;
                        margin = new Vector2(marginValue_FromPercentage, marginValue_FromPercentage);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}

public class MBAQuestElement_Property : MBAQuestElement
{
    string id = "";
    string building = "";
    string subject = "";
    string outcome = "";

    public string ID { get { return id; } set { id = value; } }
    public string Building { get { return building; } set { building = value; } }
    public string Subject { get { return subject; } set { subject = value; } }
    public string Outcome { get { return outcome; } set { outcome = value; } }

    public MBAQuestElement_Property(XmlReader reader) 
    {
        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "id":
                    id = reader.Value;
                    break;
                case "location":
                    building = reader.Value;
                    break;
                case "subject":
                    subject = reader.Value;
                    break;
                case "outcome":
                    outcome = reader.Value;
                    break;
                default:
                    break;
            }
        }

        //Customize object
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.EndElement:
                    if (reader.Name == "property")
                        return;
                    break;
            }
        }
    }
    public void print()
    {
        Debug.Log("[Property]ID=" + id + " Name=" + elementName);
    }
}

public class MBAQuestElement_Choices : MBAQuestElement
{
    List<MBAQuestElement_Choice> content = new List<MBAQuestElement_Choice>();
    public List<MBAQuestElement_Choice> Content { get { return content; } }

    public MBAQuestElement_Choices() { }
    public MBAQuestElement_Choices(float x, float y, string relativeValue)
    {
        //elementName = reader.Value;
        localPosition.x = x;
        localPosition.y = y;
        relative = relativeValue;
        //width = Single.Parse(reader.Value);
        //height = Single.Parse(reader.Value);
    }
    public MBAQuestElement_Choices(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    localPosition.x = Single.Parse(reader.Value);
                    break;
                case "y":
                    localPosition.y = Single.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                default:
                    break;
            }
        }
        //Customize label
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "choice":
                            content.Add(new MBAQuestElement_Choice(readerSubtree));
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

public class MBAQuestElement_Choice : MBAQuestElement
{
    bool isright;
    string text;

    public bool isRight { get { return isright; } set { isright = value; } }
    public string Text { get { return text; } set { text = value; } }

    public MBAQuestElement_Choice(XmlReader reader) 
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    localPosition.x = Single.Parse(reader.Value);
                    break;
                case "y":
                    localPosition.y = Single.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                case "isright":
                    isright = reader.Value == "true" ? true : false;
                    break;
                default:
                    break;
            }
        }
        //Customize label
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    text = reader.Value;
                    break;
                case XmlNodeType.Element:
                default:
                    break;
            }
        }
    }
}

public class MBAQuestElement_Dropdown : MBAQuestElement
{
    List<MBAQuestElement_Choice> choices = new List<MBAQuestElement_Choice>();
    string rightanswer;

    public List<MBAQuestElement_Choice> Choices { get { return choices; } /*set { choices = value; }*/ }


    public MBAQuestElement_Dropdown(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                default:
                    break;
            }
        }
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "choice":
                            choices.Add(new MBAQuestElement_Choice(reader));
                            break;
                    }break;
                default:
                    break;
            }
        }
    }

}

public class MBAQuestElement_Table:MBAQuestElement
{
    List<MBAQuestElement_TableTr> trs = new List<MBAQuestElement_TableTr>();
    public List<MBAQuestElement_TableTr> Trs { get { return trs; } set { trs = value; } }

    public MBAQuestElement_Table(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    base.localPosition.x = float.Parse(reader.Value);
                    break;
                case "y":
                    base.localPosition.y = float.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                default:
                    break;
            }
        }
        //Sub elements
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "tr":
                            trs.Add(new MBAQuestElement_TableTr(readerSubtree));
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

}
public class MBAQuestElement_TableTr:MBAQuestElement
{
    List<MBAQuestElement_TableTd> tds = new List<MBAQuestElement_TableTd>();
    public List<MBAQuestElement_TableTd> Tds { get { return tds; } set { tds = value; } }

    public MBAQuestElement_TableTr(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    base.localPosition.x = float.Parse(reader.Value);
                    break;
                case "y":
                    base.localPosition.y = float.Parse(reader.Value);
                    break;
                case "width":
                    width = Single.Parse(reader.Value);
                    break;
                case "height":
                    height = Single.Parse(reader.Value);
                    break;
                default:
                    break;
            }
        }

        //Sub elements
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "td":
                            tds.Add(new MBAQuestElement_TableTd(readerSubtree));
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

}
public class MBAQuestElement_TableTd : MBAQuestElement
{
    List<object> elements = new List<object>();
    public List<object> Elements { get { return elements; } set { elements = value; } }

    public MBAQuestElement_TableTd(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Sub elements
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    elements.Add(readerSubtree.Value);
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "blank":
                            elements.Add(new MBAQuestElement_Blank(readerSubtree));
                            break;
                        case "dropdown":
                            elements.Add(new MBAQuestElement_Dropdown(readerSubtree));
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

public class MBAQuestElement_Curve : MBAQuestElement
{
    Color color;
    int rightanswer_x;
    List<Vector2> points = new List<Vector2>();

    public Color Color { get { return color; } set { color = value; } }
    public int RightAnswer_x { get { return rightanswer_x; } set { rightanswer_x = value; } }
    public List<Vector2> Points { get { return points; } set { points = value; } }

    public MBAQuestElement_Curve(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    base.localPosition.x = float.Parse(reader.Value);
                    break;
                case "y":
                    base.localPosition.y = float.Parse(reader.Value);
                    break;
                case "width":
                    Single.TryParse(reader.Value, out width);
                    break;
                case "height":
                    Single.TryParse(reader.Value, out height);
                    break;
                case "color":
                    switch (reader.Value)
                    {
                        case "red":
                            color = Color.red;
                            break;
                        case "green":
                            color = Color.green;
                            break;
                        case "blue":
                            color = Color.blue;
                            break;
                        case "black":
                            color = Color.black;
                            break;
                        case "cyan":
                            color = Color.cyan;
                            break;
                        case "magenta":
                            color = Color.magenta;
                            break;
                        case "gray":
                            color = Color.gray;
                            break;
                        case "grey":
                            color = Color.grey;
                            break;
                        case "yellow":
                            color = Color.yellow;
                            break;
                        case "white":
                            color = Color.white;
                            break;
                    }
                    break;
                case "rightanswer-x":
                    int.TryParse(reader.Value, out rightanswer_x);
                    break;
            }
        }

        //Sub elements
        while (readerSubtree.Read())
        {
            Vector2 currPoint = Vector2.zero;
            bool isPoint = false;
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "point":
                            isPoint = true;
                            while (readerSubtree.MoveToNextAttribute())
                            {
                                switch (readerSubtree.Name)
                                {
                                    case "xvalue":
                                        float xvalue;
                                        Single.TryParse(readerSubtree.Value, out xvalue);
                                        currPoint = new Vector2(xvalue, currPoint.y);
                                        break;
                                    case "yvalue":
                                        float yvalue;
                                        Single.TryParse(readerSubtree.Value, out yvalue);
                                        currPoint = new Vector2(currPoint.x, yvalue);
                                        break;
                                }
                            }
                            break;
                    }
                    break;
                default:
                    break;
            }
            if (isPoint)
                points.Add(currPoint);
        }
    }
}
public class MBAQuestElement_Chart : MBAQuestElement
{
    string xname;
    string yname;
    float x_min;
    float x_max;
    float y_min;
    float y_max;
    int x_division;
    int y_division;
    string labelmode = "xy";
    string editable = "";

    Vector3 scale = Vector3.one;
    List<MBAQuestElement_Curve> curves = new List<MBAQuestElement_Curve>();
    List<string> replacelist_x = new List<string>();
    List<string> replacelist_y = new List<string>();
    public string LabelMode { get { return labelmode; } set { labelmode = value; } }
    public string Editable { get { return editable; }}

    public string NameX { get { return xname; }}
    public string NameY { get { return yname; } }
    public float X_min { get { return x_min; } }
    public float X_max { get { return x_max; } }
    public float Y_min { get { return y_min; } }
    public float Y_max { get { return y_max; } }
    public int X_division { get { return x_division; } }
    public int Y_division { get { return y_division; } }
    public Vector3 Scale { get { return scale; } }
    public List<MBAQuestElement_Curve> Curves { get { return curves; } set { curves = value; } }
    public List<string> ValueList_X { get { return replacelist_x; } }
    public List<string> ValueList_Y { get { return replacelist_y; } }

    public MBAQuestElement_Chart(XmlReader reader)
    {
        reader.MoveToElement();
        XmlReader readerSubtree = reader.ReadSubtree();

        //Attribute
        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    elementName = reader.Value;
                    break;
                case "x":
                    base.localPosition.x = float.Parse(reader.Value);
                    break;
                case "y":
                    base.localPosition.y = float.Parse(reader.Value);
                    break;
                case "width":
                    Single.TryParse(reader.Value, out width);
                    break;
                case "height":
                    Single.TryParse(reader.Value, out height);
                    break;
                case "xname":
                    xname = reader.Value;
                    break;
                case "yname":
                    yname = reader.Value;
                    break;
                case "x-min":
                    Single.TryParse(reader.Value, out x_min);
                    break;
                case "x-max":
                    Single.TryParse(reader.Value, out x_max);
                    break;
                case "y-min":
                    Single.TryParse(reader.Value, out y_min);
                    break;
                case "y-max":
                    Single.TryParse(reader.Value, out y_max);
                    break;
                case "x-division":
                    int.TryParse(reader.Value, out x_division);
                    break;
                case "y-division":
                    int.TryParse(reader.Value, out y_division);
                    break;
                case "scale":
                    float scaleValue;
                    Single.TryParse(reader.Value, out scaleValue);
                    scale = new Vector2(scaleValue, scaleValue);
                    break;
                case "scale-x":
                    float scaleValue_x;
                    Single.TryParse(reader.Value, out scaleValue_x);
                    scale = new Vector2(scaleValue_x, scale.y);
                    break;
                case "scale-y":
                    float scaleValue_y;
                    Single.TryParse(reader.Value, out scaleValue_y);
                    scale = new Vector2(scale.x, scaleValue_y);
                    break;
                case "editable":
                    editable = reader.Value;
                    break;
                default:
                    break;
            }
        }
        //Sub elements
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "curve":
                            curves.Add(new MBAQuestElement_Curve(readerSubtree));
                            break;
                        case "xvaluereplace":
                            while (readerSubtree.MoveToNextAttribute())
                            {
                                switch (readerSubtree.Name)
                                {
                                    case "value":
                                        replacelist_x.Add(readerSubtree.Value);
                                        break;
                                }
                            }
                            if (labelmode == "xy")
                                labelmode = "y";
                            else if (labelmode == "x")
                                labelmode = "";
                            break;
                        case "yvaluereplace":
                            while (readerSubtree.MoveToNextAttribute())
                            {
                                switch (readerSubtree.Name)
                                {
                                    case "value":
                                        replacelist_y.Add(readerSubtree.Value);
                                        break;
                                }
                            }
                            if (labelmode == "xy")
                                labelmode = "x";
                            else if (labelmode == "y")
                                labelmode = "";
                            break;

                    }
                    break;
                default:
                    break;
            }
        }
    }

}


/*
public class MBAQuest_Help
{
    string name = "HELP";
    ArrayList helpContent;
    public string Name { get { return name; } set { name = value; } }
    public ArrayList Content { get { return helpContent; } set { helpContent = value; } }

    public MBAQuest_Help(XmlReader reader)
    {
        XmlReader readerSubtree = reader.ReadSubtree();
        if (readerSubtree == null)
            return;

        while (reader.MoveToNextAttribute())
        {
            switch (reader.Name)
            {
                case "name":
                    name = reader.Value;
                    break;
            }
        }
        
        helpContent = new ArrayList();
        while (readerSubtree.Read())
        {
            switch (readerSubtree.NodeType)
            {
                case XmlNodeType.Text:
                    helpContent.Add(readerSubtree.Value);
                    break;
                case XmlNodeType.Element:
                    switch (readerSubtree.Name)
                    {
                        case "img":
                            while (readerSubtree.MoveToNextAttribute())
                            {
                                switch (readerSubtree.Name)
                                {
                                    case "src":
                                        helpContent.Add(new MBAQuestElement_StaticImage(readerSubtree.Value));
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
    }

    void print()
    {
        foreach (object obj in helpContent)
        {
            switch (obj.GetType().Name)
            {
                case "String":
                    Debug.Log((string)obj);
                    break;
                case "StaticImage":
                    Debug.Log(((MBAQuestElement_StaticImage)obj).Url);
                    break;

            }
        }
    }
}
*/
/*
class MBAQuestElement_StaticImage
{
    string url;

    public string Url { get { return url; } set { url = value; } }

    public MBAQuestElement_StaticImage(string url)
    {
        Url = url;
    }

}*/

