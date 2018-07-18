using UnityEngine;
using UnityEngine.UI;

public class ChannelButtonManager : MonoBehaviour {
    Transform[] buttonChildren;
    public Twitch_EMT twitchPlayer;
    ChannelButton chosenButton;
    public TVUIStateManager stateManager;

    public int selectedInt = 0;
    int actualInt = 0;
    int previousInt = 0;

    public float moveNum = 0;
    public Text title;

    public bool play;

	void Start () {
        buttonChildren = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)

        {
            buttonChildren[i] = transform.GetChild(i);
        }

        // This is where color and size is initially set.
        buttonChildren[0].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        buttonChildren[0].transform.localScale = new Vector3(.25f, .25f, .25f);

        for (int i = 1; i < buttonChildren.Length; i++)
        {
            buttonChildren[i].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 115);
            buttonChildren[i].transform.localScale = new Vector3(.2f, .2f, .2f);
        }
    }

    private void OnEnable()
    {
        // And here we reset for the user each time they use the channel button menu.

        selectedInt = 0;
        title.text = transform.GetChild(0).gameObject.name;
    }

    void Update () {
        if (play)
        {
            PlaySelected();
            play = false;
        }
        // This gets the number of the child we're going to use from selectedInt, the var that is only used for animating the rotation of the icons.
        if (selectedInt >= buttonChildren.Length || selectedInt < 0)
        {
            actualInt = selectedInt % buttonChildren.Length;
            if (Mathf.Sign(actualInt) == -1)
            {
                actualInt += buttonChildren.Length;
            }
        }
        else
        {
            actualInt = selectedInt;
        }

        // This is where we offset the location of the icons when moving between selections.
        if (moveNum != selectedInt)
        {
            if (Mathf.Round(moveNum * 10) / 10 == selectedInt)
            {
                moveNum = selectedInt;
            }
            else
            {
                moveNum = Mathf.Lerp(moveNum, selectedInt, .12f);
            }
        }

        // This sets the color and size of the icons based on the selection.
        if (previousInt != actualInt)
        {
            ColorChange();
        }

        // And here is where the positions are set.
        for (int i = 0; i < buttonChildren.Length; i++)
        {
            float theta = (2 * Mathf.PI / buttonChildren.Length) * (i - moveNum);
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            buttonChildren[i].localPosition = new Vector3(xPos, yPos, 0f) * .525f;
        }
	}

    // And of course, here's the actual code used to set color and size. It also sets the text of the title now.
    void ColorChange()
    {
        buttonChildren[actualInt].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        buttonChildren[actualInt].transform.localScale = new Vector3(.25f, .25f, .25f);

        buttonChildren[previousInt].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 115);
        buttonChildren[previousInt].transform.localScale = new Vector3(.2f, .2f, .2f);

        previousInt = actualInt;

        title.text = buttonChildren[actualInt].gameObject.name;
    }

    public void PlaySelected()
    {
        chosenButton = buttonChildren[actualInt].gameObject.GetComponent<ChannelButton>();
        if (chosenButton)
        {
            twitchPlayer.PlayVideo(chosenButton.channel, chosenButton.source);
            stateManager.ChangeState(-1);
        }
        else
        {
            stateManager.ChangeState(0);
        }
    }
}
