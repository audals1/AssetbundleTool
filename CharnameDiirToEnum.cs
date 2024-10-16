using UnityEngine;
using System;
using System.IO;
using System.Text;

public class CharnameDiirToEnum : MonoBehaviour
{
    // 열거형 정의


    public void GetCharNameToEnum()
    {
        // 폴더 경로
        string folderPath = "Assets/2Dasset/";

        // 폴더 내의 모든 하위 디렉토리 가져오기
        string[] directories = Directory.GetDirectories(folderPath);

        // StringBuilder를 사용하여 모든 디렉토리 이름을 한 번에 출력
        StringBuilder sb = new StringBuilder();
        foreach (string directoryPath in directories)
        {
            // 디렉토리 이름 가져오기
            string directoryName = Path.GetFileName(directoryPath);

            // StringBuilder에 디렉토리 이름 추가
            sb.AppendLine(directoryName);
        }

        // StringBuilder에 저장된 모든 내용을 Debug.Log로 출력
        Debug.Log(sb.ToString());
    }
}