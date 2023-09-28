using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Status Text")]
    [SerializeField]private TextMeshProUGUI statusText;

    public void UpdateStatus(string status)
    {
        StartCoroutine("UpdateStatusRoutine", status);
    }

    private IEnumerator UpdateStatusRoutine(string status)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = status;
        yield return new WaitForSeconds(3);
        statusText.gameObject.SetActive(false);
    }

    public void OnClickAutoSolve()
    {
        if (GameManager.Instance.gameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        GameManager.Instance.RunSolver();
    }

}
