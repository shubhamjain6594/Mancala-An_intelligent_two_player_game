using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public Text[] TextFields;
    private GamePlay gp;
    public static Manager Instance;

	void Awake () { gp = GetComponent<GamePlay>(); Instance = this; }
    public void GameInput(int pitIndex){ gp.UserInput(pitIndex); }
    public void RestartButton() { gp.ApplyRestart(); }
    public void UndoButton() { gp.ApplyUndo(); }
    public void SetTextField(int textFieldIndex, string text) { TextFields[textFieldIndex].text = text; }
}
