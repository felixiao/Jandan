using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LitJson;

namespace HtmlAgilityPack
{
    public class HtmlDownload : MonoBehaviour
    {
        public RawImage rawImg;
        public Text textAuthor;
        public Text textOO;
        public Text textXX;
        public Text textPage;
        public GameObject panelTop;
        public GameObject panelBottom;
        //public string url;
        WWW www;
        string pageUrl = "http://jandan.net/ooxx";
        string html;
        HtmlDocument doc = new HtmlDocument();
        List<Floor> floors = new List<Floor>();
        int curFloor=0,curImgIndex=0,curImgCount=1;

        string imgUrl;
        int pageLatest;
        int curPage;
        float screenRatio;
        bool toggle;
        // Use this for initialization
        IEnumerator Start()
        {
            screenRatio = (float)Screen.width / (float)Screen.height;

            www = new WWW(pageUrl);
            yield return www;

            doc.LoadHtml(www.text);
            string page = doc.DocumentNode.SelectSingleNode("//span[@class=\"current-comment-page\"]").InnerText;
            pageLatest = int.Parse(page.Substring(1,page.Length-2));
            curPage = pageLatest;
            HtmlNode htmlnode = doc.DocumentNode.SelectSingleNode("//ol[@class=\"commentlist\"]");
            HtmlNodeCollection commentNodes = htmlnode.SelectNodes(".//li[@id]");
            int childCount = commentNodes.Count;
            for (int i = childCount - 1; i >= 0; i--)
            {
                if (commentNodes[i].Attributes.Contains("id"))
                {
                    HtmlNode cNode = commentNodes[i].SelectSingleNode(".//div[@class=\"row\"]");
                    Floor f = new Floor();
                    f.page = curPage;
                    f.comment_id = int.Parse(commentNodes[i].Id.Substring(8));
                    f.author = cNode.SelectSingleNode(".//strong").InnerText;
                    cNode = cNode.SelectSingleNode(".//div[@class=\"text\"]");
                    f.floor_id = int.Parse(cNode.FirstChild.FirstChild.InnerText);
                    f.oo = int.Parse(cNode.SelectSingleNode(string.Format(".//span[@id=\"cos_support-{0}\"]", f.comment_id)).InnerText);
                    f.xx = int.Parse(cNode.SelectSingleNode(string.Format(".//span[@id=\"cos_unsupport-{0}\"]", f.comment_id)).InnerText);
                    HtmlNodeCollection imgNodes = cNode.SelectNodes(".//img[@src]");
                    foreach (HtmlNode imgNode in imgNodes)
                    {
                        if (imgNode.Attributes.Contains("src"))
                        {
                            f.urls.Add(imgNode.Attributes["src"].Value);
                            www = new WWW(imgNode.Attributes["src"].Value);
                            yield return www;

                            f.pics.Add(www.texture);
                        }
                    }
                    floors.Add(f);
                }
            }
            curFloor = floors.Count - 1;
            curImgIndex = floors[floors.Count - 1].urls.Count - 1;
            UpdateUI();
        }

        IEnumerator ProcessHtml()
        {
            floors.Clear();
            www = new WWW(pageUrl);
            Debug.Log("ProcessHtml " + pageUrl);
            yield return www;

            doc.LoadHtml(www.text);
            
            HtmlNode htmlnode = doc.DocumentNode.SelectSingleNode("//ol[@class=\"commentlist\"]");
            HtmlNodeCollection commentNodes = htmlnode.SelectNodes(".//li[@id]");
            int childCount = commentNodes.Count;
            for (int i = 0; i < childCount; i++)
            {
                if (commentNodes[i].Attributes.Contains("id"))
                {
                    HtmlNode cNode = commentNodes[i].SelectSingleNode(".//div[@class=\"row\"]");
                    Floor f = new Floor();
                    f.page = curPage;
                    f.comment_id = int.Parse(commentNodes[i].Id.Substring(8));
                    f.author = cNode.SelectSingleNode(".//strong").InnerText;
                    cNode = cNode.SelectSingleNode(".//div[@class=\"text\"]");
                    f.floor_id = int.Parse(cNode.FirstChild.FirstChild.InnerText);
                    f.oo = int.Parse(cNode.SelectSingleNode(string.Format(".//span[@id=\"cos_support-{0}\"]", f.comment_id)).InnerText);
                    f.xx = int.Parse(cNode.SelectSingleNode(string.Format(".//span[@id=\"cos_unsupport-{0}\"]", f.comment_id)).InnerText);
                    HtmlNodeCollection imgNodes = cNode.SelectNodes(".//img[@src]");
                    foreach (HtmlNode imgNode in imgNodes)
                    {
                        if (imgNode.Attributes.Contains("src"))
                        {
                            f.urls.Add(imgNode.Attributes["src"].Value);
                            www = new WWW(imgNode.Attributes["src"].Value);
                            yield return www;

                            f.pics.Add(www.texture);
                        }
                    }
                    floors.Add(f);
                }
            }
            UpdateUI();
        }
        // Update is called once per frame
        void Update()
        {
            float ratio = (float)Screen.width / (float)Screen.height;

            if (!screenRatio.Equals(ratio))
            {
                screenRatio = ratio;
                UpdateUI();
            }
            if(Input.GetMouseButtonDown(0)){
                Vector3 pos=Input.mousePosition;
                if (pos.x > (float)Screen.width * 4 / 5)
                    ShowNext();
                else if (pos.x < (float)Screen.width / 5)
                    ShowPrev();
                else
                {
                    toggle = !toggle;
                    ToggleInfo();
                }
            }
            if (Input.touchCount>0&&Input.touches[0].phase == TouchPhase.Began)
            {
                Vector2 pos = Input.touches[0].position;
                if (pos.x > (float)Screen.width * 4 / 5)
                    ShowNext();
                else if (pos.x < (float)Screen.width / 5)
                    ShowPrev();
                else
                {
                    toggle = !toggle;
                    ToggleInfo();
                }
            }
        }
        void ToggleInfo()
        {
            textAuthor.enabled = toggle;
            textOO.enabled = toggle;
            textXX.enabled = toggle;
            textPage.enabled = toggle;
            panelTop.SetActive(toggle);
            panelBottom.SetActive(toggle);
        }
        void ShowNext()
        {
            curImgIndex++;
            if (curImgIndex >= curImgCount)
            {
                curFloor++;
                if (curFloor >= floors.Count)
                {
                    Debug.Log("ShowNextPage " + pageUrl);
                    ShowNextPage();
                    curFloor = 0;
                }
                curImgIndex = 0;
                curImgCount = floors[curFloor].urls.Count;
            }
            UpdateUI();
        }
        void ShowPrev()
        {
            curImgIndex--;
            if (curImgIndex < 0)
            {
                curFloor--;
                if (curFloor < 0)
                {
                    Debug.Log("ShowPrevPage " + pageUrl);
                    ShowPrevPage();
                    curFloor = floors.Count - 1;
                }
                curImgCount = floors[curFloor].urls.Count;
                curImgIndex = curImgCount - 1;
            }

            UpdateUI();
        }
        void ShowPrevPage()
        {
            curPage--;
            if (curPage < 900)
            {
                curPage = 900;
                return;
            }
            pageUrl = string.Format("http://jandan.net/ooxx/page-{0}#comments", curPage);
            Debug.Log("ProcessHtml " + pageUrl);
            ProcessHtml();
        }
        void ShowNextPage()
        {
            curPage++;
            if (curPage > pageLatest)
            {
                curPage = pageLatest;
                return;
            }
            pageUrl = string.Format("http://jandan.net/ooxx/page-{0}#comments",curPage);
            Debug.Log("ProcessHtml " + pageUrl);
            ProcessHtml();
        }
        void UpdateUI()
        {
            textAuthor.text = floors[curFloor].author;
            textOO.text = floors[curFloor].oo.ToString();
            textXX.text = floors[curFloor].xx.ToString();
            textPage.text = floors[curFloor].page + " - " + floors[curFloor].floor_id.ToString();
            
            rawImg.texture = floors[curFloor].pics[curImgIndex];
            //rawImg.SetNativeSize();
            float ratio =(float) rawImg.texture.width / (float)rawImg.texture.height;
            if (ratio > screenRatio)
            {
                rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)Screen.width/ratio);
                rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            }
            else
            {
                rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
                rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)Screen.height*ratio);
            }

        }
        public string OutputJson()
        {
            return JsonMapper.ToJson(floors);
        }
        public void ReadJson(string json)
        {
            floors.Clear();
            floors = JsonMapper.ToObject<List<Floor>>(json);
        }
    }
    
    public class Floor
    {
        public int comment_id { get; set; }
        public int floor_id { get; set; }
        public int page { get; set; }
        public string author { get; set; }
        public int oo { get; set; }
        public int xx { get; set; }
        public List<Texture> pics = new List<Texture>();
        public List<string> urls = new List<string>();
    }
}

