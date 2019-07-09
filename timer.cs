using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    public Text timerLabel;
	public Text introText;
	public Image introBackground;

	bool debugMode = false;

    private float time = 620;

	void Start() {
		if (!debugMode) {
			introText.text = "Your horse collapsed as you travelled across the desert, stranding you. " + 
				"Fortunately you know that your friend is coming through the area tonight and will be able " +
				"to rescue you, but until then you need to stay alive. Food can be used to restore your health. " +
				"Monsters roam the area, but won't attack unless you get too close - but you have a gun with " +
				"you to protect yourself. Friendly people live in the region, who may be looking for help or " + 
				"willing to sell you goods. Finally, this area is known to be full of gold. Good luck! \n\n" +
				"Controls: Use WASD to move and the mouse to look around. Press spacebar to jump, click to fire " +
				"your weapon, and press alt to use food to heal damage. Clicking will also make dialog boxes disappear.";
			introBackground.color = new Color (1f, 1f, 1f, 1f);
		}
	}

    void Update()
    {
        time -= Time.deltaTime;

		var minutes = (time / 60); //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;//Use the euclidean division for the seconds.
        var fraction = (time * 100) % 100;

        //update the label value
        timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

		if (time <= 600) {
			introText.text = "";
			introBackground.color = new Color (0f, 0f, 0f, 0f);
		}
    }
}