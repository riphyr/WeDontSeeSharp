using UnityEngine;
using System;
using System.Speech.Recognition;

public class SimpleSpeechRecognition : MonoBehaviour
{
    private SpeechRecognitionEngine recognizer;
    private float confidenceThreshold = 0.65f;

    void Start()
    {
        StartRecognition();
    }

    void StartRecognition()
    {
        try
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.SetInputToDefaultAudioDevice();

            Choices commands = new Choices();
            commands.Add(new string[]
            {
                "switch the light",
                "are you here",
                "what is your name",
                "i am here to defeat you",
                "leave me alone"
            });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(commands);
            Grammar g = new Grammar(gb);
            recognizer.LoadGrammar(g);

            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1);
            recognizer.BabbleTimeout = TimeSpan.FromSeconds(1.5);

            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }
        catch (Exception e)
        {
            Debug.LogError($"🚨 Erreur reconnaissance vocale : {e.Message}");
        }
    }

    private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Confidence < confidenceThreshold)
        {
            Debug.Log($"❌ Faux positif ignoré : {e.Result.Text} (Confiance : {e.Result.Confidence * 100:F1}%)");
            return;
        }

        if (e.Result.Text.Length < 4)
        {
            Debug.Log($"🔇 Ignoré (mot trop court) : {e.Result.Text}");
            return;
        }

        Debug.Log($"✅ Détection confirmée : {e.Result.Text} ({e.Result.Confidence * 100:F1}%)");

        switch (e.Result.Text)
        {
            case "are you here":
                Debug.Log("Yes !");
                break;
            case "switch the light":
                Debug.Log("Lumière switch !");
                break;
            case "i am here to defeat you":
                Debug.Log("I will do it first !");
                break;
            case "leave me alone":
                Debug.Log("Never !");
                break;
            case "what is your name":
                Debug.Log("I'm Fabrice !");
                break;
        }
    }

    void OnApplicationQuit()
    {
        if (recognizer != null)
        {
            recognizer.Dispose();
        }
    }
}
