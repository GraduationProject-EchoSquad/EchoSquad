using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Whisper.Utils;
using Debug = System.Diagnostics.Debug;

namespace Whisper.Samples
{
    /// <summary>
    /// Takes audio clip and make a transcription.
    /// </summary>
    public class AudioClipListDemo : MonoBehaviour
    {
        public WhisperManager manager;
        public List<AudioClip> clips;
        public string referenceText;
        public bool streamSegments = true;
        public bool echoSound = true;
        public bool printLanguage = true;

        [Header("UI")] public Button button;
        public Text outputText;
        public Text timeText;
        public ScrollRect scroll;
        public Dropdown languageDropdown;
        public Toggle translateToggle;

        private string _buffer;

        private void Awake()
        {
            manager.OnNewSegment += OnNewSegment;
            manager.OnProgress += OnProgressHandler;

            button.onClick.AddListener(ButtonPressed);
            languageDropdown.value = languageDropdown.options
                .FindIndex(op => op.text == manager.language);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            translateToggle.isOn = manager.translateToEnglish;
            translateToggle.onValueChanged.AddListener(OnTranslateChanged);
        }

        public async void ButtonPressed()
        {
            float avgTime = 0f;
            float avgRate = 0f;
            float avgWER = 0f;
            float avgCER = 0f;
            foreach (var clip in clips)
            {
                _buffer = "";
                if (echoSound)
                    AudioSource.PlayClipAtPoint(clip, Vector3.zero);

                var sw = new Stopwatch();
                sw.Start();

                var res = await manager.GetTextAsync(clip);
                if (res == null || !outputText)
                    return;

                var time = sw.ElapsedMilliseconds;
                avgTime += time;
                var rate = clip.length / (time * 0.001f);
                avgRate += rate;
                var WER = CalculateWER(referenceText, res.Result);
                avgWER += WER;
                var CER = CalculateCER(referenceText, res.Result);
                avgCER += CER;
                //timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

                var text = res.Result;
                if (printLanguage)
                    text += $"\n\nLanguage: {res.Language}";

                outputText.text = text;
                UiUtils.ScrollDown(scroll);
            }

            avgTime /= clips.Count;
            avgRate /= clips.Count;
            avgWER /= clips.Count;
            avgCER /= clips.Count;
            timeText.text = $"Time: {avgTime} ms\nRate: {avgRate:F1}x\nWER: {avgWER * 100}%\nCER: {avgCER * 100}%";
        }

        private void OnLanguageChanged(int ind)
        {
            var opt = languageDropdown.options[ind];
            manager.language = opt.text;
        }

        private void OnTranslateChanged(bool translate)
        {
            manager.translateToEnglish = translate;
        }

        private void OnProgressHandler(int progress)
        {
            if (!timeText)
                return;
            timeText.text = $"Progress: {progress}%";
        }

        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text;
            outputText.text = _buffer + "...";
            UiUtils.ScrollDown(scroll);
        }

        // 특수문자 제거 함수
        private static string RemovePunctuation(string input)
        {
            return Regex.Replace(input, @"[^\w\s가-힣]", ""); // 영어, 숫자, 한글, 공백만 남기기
        }

        // WER 계산 함수
        public static float CalculateWER(string reference, string hypothesis)
        {
            reference = RemovePunctuation(reference);
            hypothesis = RemovePunctuation(hypothesis);
            // Reference와 Hypothesis를 공백 기준으로 나눈 단어 배열로 변환
            var referenceWords = reference.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var hypothesisWords = hypothesis.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // 동적 계획법을 사용하여 WER을 계산 (편집 거리 알고리즘)
            var dp = new int[referenceWords.Length + 1, hypothesisWords.Length + 1];

            for (int i = 0; i <= referenceWords.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= hypothesisWords.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= referenceWords.Length; i++)
            {
                for (int j = 1; j <= hypothesisWords.Length; j++)
                {
                    int cost = referenceWords[i - 1] == hypothesisWords[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            // WER 계산: 편집 거리 / 참조 단어 수
            int editDistance = dp[referenceWords.Length, hypothesisWords.Length];
            return (float)editDistance / referenceWords.Length;
        }

        // CER 계산 함수
        public static float CalculateCER(string reference, string hypothesis)
        {
            reference = RemovePunctuation(reference);
            hypothesis = RemovePunctuation(hypothesis);
            // Reference와 Hypothesis를 문자 배열로 변환
            var referenceChars = reference.Replace(" ", "").ToCharArray();
            var hypothesisChars = hypothesis.Replace(" ", "").ToCharArray();

            // 동적 계획법을 사용하여 CER을 계산 (편집 거리 알고리즘)
            var dp = new int[referenceChars.Length + 1, hypothesisChars.Length + 1];

            for (int i = 0; i <= referenceChars.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= hypothesisChars.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= referenceChars.Length; i++)
            {
                for (int j = 1; j <= hypothesisChars.Length; j++)
                {
                    int cost = referenceChars[i - 1] == hypothesisChars[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            // CER 계산: 편집 거리 / 참조 문자 수
            int editDistance = dp[referenceChars.Length, hypothesisChars.Length];
            return (float)editDistance / referenceChars.Length;
        }
    }
}