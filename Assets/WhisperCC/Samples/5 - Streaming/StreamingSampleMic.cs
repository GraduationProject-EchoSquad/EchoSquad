using System;
using LLMUnitySamples;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Stream transcription from microphone input.
    /// </summary>
    public class StreamingSampleMic : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        [SerializeField] private FunctionCalling functionCalling;
    
        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text text;
        //public ScrollRect scroll;
        private WhisperStream _stream;

        private async void Start()
        {
            microphoneRecord = MicrophoneManager.Instance.microphoneRecord;
            _stream = await whisper.CreateStream(microphoneRecord);
            if (_stream != null)
            {
                _stream.OnResultUpdated += OnResult;
                _stream.OnSegmentUpdated += OnSegmentUpdated;
                _stream.OnSegmentFinished += OnSegmentFinished;
                _stream.OnStreamFinished += OnFinished;
            }

            microphoneRecord.OnRecordStop += OnRecordStop;
            if (button)
            {
                button.onClick.AddListener(OnButtonPressed);
            }
        }

        private void OnDestroy()
        {
            microphoneRecord.OnRecordStop -= OnRecordStop;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                OnButtonPressed();
            }else if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                OnButtonPressed();
            }
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
                PubSubManager.Instance.Publish(PubSubEvent.OnMicStart);
            }
            else
            {
                microphoneRecord.StopRecord();
                PubSubManager.Instance.Publish(PubSubEvent.OnMicEnd);
            }

            if (buttonText)
            {
                buttonText.text = microphoneRecord.IsRecording ? "Stop" : "Record";
            }
        }
    
        private void OnRecordStop(AudioChunk recordedAudio)
        {
            if (buttonText)
            {
                buttonText.text = "Record";
            }
        }
    
        private void OnResult(string result)
        {
            if (text)
            {
                text.text = result;
            }

            //UiUtils.ScrollDown(scroll);
        }
        
        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
        }
        
        private void OnSegmentFinished(WhisperResult segment)
        {
            print($"Segment finished: {segment.Result}");

            // 빈 메시지나 침묵(점만 있는 경우) 필터링
            if (IsValidCommand(segment.Result))
            {
                functionCalling?.SendCommandFromText(segment.Result);
            }
            else
            {
                print($"Ignored silence/invalid input: '{segment.Result}'");
            }
        }

        /// <summary>
        /// 유효한 명령인지 확인 (침묵이나 쓰레기 텍스트 필터링)
        /// </summary>
        private bool IsValidCommand(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // 점과 공백만 있는 경우 무시
            string cleaned = text.Replace(".", "").Replace(" ", "").Replace(",", "");
            if (string.IsNullOrEmpty(cleaned))
                return false;

            // Whisper 환청(hallucination) 필터링 - 자주 나오는 오인식 문구들
            string lower = text.ToLower().Trim();
            string[] hallucinations = {
                "thank you",
                "thanks",
                "thanks for watching",
                "thank you for watching",
                "bye",
                "goodbye",
                "see you",
                "you",
                "uh",
                "um",
                "hmm",
                "ah"
            };

            foreach (string hallucination in hallucinations)
            {
                if (lower == hallucination)
                {
                    print($"[Whisper] Detected hallucination: '{text}'");
                    return false;
                }
            }

            return true;
        }
        
        private void OnFinished(string finalResult)
        {
            print("Stream finished!");
        }
    }
}
