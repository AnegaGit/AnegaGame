/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBook : MonoBehaviour
{
    public GameObject panel;
    public Image bookImage;
    public Text textLeftSide;
    public Text textRightSide;
    public Text textSingleSide;
    public Button leftPage;
    public Button rightPage;
    public Button buttonExecute;
    public Text buttonExecuteText;
    private BookItem _bookItem;
    private BookType _bookType;
    private string[] _bookText;
    private string _bookName;
    private string _bookAuthor;
    private float displaySize = 1;
    private int currentPage;
    private bool hasTitle;
    private bool isSinglePage;
    public Sprite[] spriteCover;
    public Sprite[] spriteContent;
    Player player;
    private int _containerId;
    private int _slotId;

    public bool isShown
    {
        get { return panel.activeSelf; }
        set { panel.SetActive(value); }
    }

    // public void Initialize(BookType bookType, string bookText, string bookName = "", string bookAuthor = "")
    public void Initialize(BookItem bookItem, int containerId, int slotId)
    {
        player = Player.localPlayer;
        _bookItem = bookItem;
        _bookType = bookItem.bookType;
        _bookText = bookItem.bookText.Split(GlobalVar.bookPageSplit, StringSplitOptions.None);
        _bookName = bookItem.title;
        _bookAuthor = bookItem.author;
        _containerId = containerId;
        _slotId = slotId;

        switch (_bookType)
        {
            case BookType.Parchment:
                {
                    currentPage = 0;
                    hasTitle = false;
                    isSinglePage = true;
                    break;
                }
            case BookType.ParchmentDouble:
                {
                    currentPage = 0;
                    hasTitle = false;
                    isSinglePage = false;
                    break;
                }

            case BookType.Book2:
                {
                    if ((_bookName + _bookAuthor).Length > 0)
                    {
                        currentPage = -1;
                        hasTitle = true;
                    }
                    else
                    {
                        currentPage = 0;
                        hasTitle = false;
                    }

                    isSinglePage = false;
                    break;
                }
            default: //case BookType.Book1:
                {
                    if ((_bookName + _bookAuthor).Length > 0)
                    {
                        currentPage = -1;
                        hasTitle = true;
                    }
                    else
                    {
                        currentPage = 0;
                        hasTitle = false;
                    }
                    isSinglePage = false;
                    break;
                }
        }

        switch (player.abilities.readAndWrite)
        {
            case Abilities.Nav:
                {
                    _bookName = GlobalVar.illiterateBookName;
                    _bookAuthor = GlobalVar.illiterateBookAuthor;
                    _bookText = GlobalVar.illiterateNavBookText;
                    break;
                }
            case Abilities.Poor:
                {
                    if (_bookText[0].Length > GlobalVar.illiteratePoorMaxText)
                    {
                        string singlePage = _bookText[0].Substring(0, _bookText[0].IndexOf(" ", GlobalVar.illiteratePoorCutText)) + Environment.NewLine + GlobalVar.illiteratePoorBookText;
                        _bookText = new string[] { singlePage };
                    }
                    else
                    {
                        string singlePage = _bookText[0] + Environment.NewLine + GlobalVar.illiteratePoorBookText;
                        _bookText = new string[] { singlePage };
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
        ShowText();
        InitializeExecute(false);
        panel.SetActive(true);
    }

    public void InitializeExecute(bool canExecute, string buttonText = "Execute")
    {
        buttonExecute.gameObject.SetActive(canExecute);
        buttonExecuteText.text = buttonText;
    }

    void Resize()
    {
        int w, h, t;
        if (currentPage == -1 || isSinglePage)
        {
            w = (int)Mathf.Max(160, GlobalVar.bookInitialWidth * displaySize / 2);
        }
        else
        {
            w = (int)Mathf.Max(160, GlobalVar.bookInitialWidth * displaySize);
        }
        h = (int)(GlobalVar.bookInitialHeight * displaySize) + 50;
        t = (int)(GlobalVar.bookInitialTextSize * displaySize);
        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        textLeftSide.fontSize = t;
        textRightSide.fontSize = t;
        textSingleSide.fontSize = t;
    }

    public void Enlarge()
    {
        displaySize = Mathf.Min(GlobalVar.bookMaxSize, displaySize += GlobalVar.bookSizeStep);
        Resize();
    }
    public void Shrink()
    {
        displaySize = Mathf.Max(GlobalVar.bookMinSize, displaySize -= GlobalVar.bookSizeStep);
        Resize();
    }

    public void FistPage()
    {
        if (hasTitle)
        {
            currentPage = -1;
        }
        else
        {
            currentPage = 0;
        }
        ShowText();
    }
    public void PreviousPage()
    {
        if (hasTitle && currentPage <= 0)
            currentPage = -1;
        else if (isSinglePage)
            currentPage = Mathf.Max(0, currentPage - 1);
        else
            currentPage = Mathf.Max(0, currentPage - 2);
        ShowText();
    }
    public void NextPage()
    {
        if (currentPage == -1)
            currentPage = 0;
        else if (isSinglePage)
            currentPage = Mathf.Min(_bookText.Length - 1, currentPage + 1);
        else
            currentPage = Mathf.Min((int)((_bookText.Length - 1) / 2) * 2, currentPage + 2);
        ShowText();
    }
    public void LastPage()
    {
        if (isSinglePage)
            currentPage = _bookText.Length - 1;
        else
            currentPage = (int)((_bookText.Length - 1) / 2) * 2;
        ShowText();
    }

    void ShowText()
    {
        Resize();
        if (currentPage == -1)
        {
            bookImage.sprite = spriteCover[(int)_bookType];
            textSingleSide.text = string.Format("<b>{0}</b>" + Environment.NewLine + "by" + Environment.NewLine + "<b>{1}</b>", _bookName, _bookAuthor);
            textSingleSide.alignment = TextAnchor.MiddleCenter;
            textLeftSide.text = "";
            textRightSide.text = "";
        }
        else if (isSinglePage)
        {
            bookImage.sprite = spriteContent[(int)_bookType];
            textSingleSide.text = _bookText[currentPage];
            textSingleSide.alignment = TextAnchor.UpperLeft;
            textLeftSide.text = "";
            textRightSide.text = "";
        }
        else
        {
            bookImage.sprite = spriteContent[(int)_bookType];
            textLeftSide.text = _bookText[currentPage];
            if (_bookText.Length > (currentPage + 1))
            {
                textRightSide.text = _bookText[currentPage + 1];
            }
            else
            {
                textRightSide.text = "";
            }
            textSingleSide.text = "";
        }

        if (currentPage == -1 || (!hasTitle && currentPage == 0))
        {
            leftPage.gameObject.SetActive(false);
        }
        else
        {
            leftPage.gameObject.SetActive(true);
        }
        if (!isSinglePage && currentPage + 2 >= _bookText.Length)
        {
            rightPage.gameObject.SetActive(false);
        }
        else if (isSinglePage && currentPage + 1 >= _bookText.Length)
        {
            rightPage.gameObject.SetActive(false);
        }
        else
        {
            rightPage.gameObject.SetActive(true);
        }
    }

    public void Execute()
    {
        _bookItem.ExecuteBook(_containerId, _slotId);
        panel.SetActive(false);
    }

    public enum BookType
    {
        Parchment = 0,
        ParchmentDouble = 1,
        Book1 = 2,
        Book2 = 3
    }
}
