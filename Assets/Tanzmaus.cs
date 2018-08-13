using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;
using Midi;

public class Tanzmaus : MonoBehaviour {

	public static Tanzmaus Instance;
	public string DeviceName = "MIDIIN4 (mio4)";
	public Channel DeviceChannel = Channel.Channel10;
	private InputDevice InputDevice;
	
	public KickState Kick = new KickState();
	public struct KickState {
		public bool NoteOn;
		public float Attack;
		public float Decay;
		public float Pitch;
		public float Tune;
		public float Noise;
	}
	
	public SnareState Snare = new SnareState();
	public struct SnareState {
		public bool NoteOn;
		public float NoiseDecay;
		public float Noise;
		public float Tune;
	}
	
	public RimshotState Rimshot = new RimshotState();
	public struct RimshotState {
		public bool NoteOn;
	}
	
	public ClapState Clap = new ClapState();
	public struct ClapState {
		public bool NoteOn;
		public float Filter;
		public float Decay;
	}
	
	public TomsState Toms = new TomsState();
	public struct TomsState {
		public bool NoteOn;
		public float Attack;
		public float Decay;
		public float Pitch;
		public float Tune;
	}
	
	public SampleState Sample1 = new SampleState();
	public SampleState Sample2 = new SampleState();
	public struct SampleState {
		public bool NoteOn;
		public bool NoteOnAlt;
		public float Tune;
		public float Decay;
	}

	// Use this for initialization
	void Start () {
		Instance = this;
		
        foreach (InputDevice inputDevice in InputDevice.InstalledDevices)
        {
            if (inputDevice.Name.ToLower().Equals(DeviceName.ToLower()))
            {
                InputDevice = inputDevice;
                break;
            }
        }
        if (InputDevice != null)
        {
            InputDevice.Open();
            InputDevice.StartReceiving(null);
            InputDevice.NoteOn += RouteNoteOn;
			InputDevice.ControlChange += RouteControlChange;
            Debug.Log("Opened MIDI Device");
        }
	}

	void Update() {
		if (Kick.NoteOn) {
			Kick.NoteOn = false;
		}
		if (Snare.NoteOn) {
			Snare.NoteOn = false;
		}
		if (Rimshot.NoteOn) {
			Rimshot.NoteOn = false;
		}
		if (Clap.NoteOn) {
			Clap.NoteOn = false;
		}
		if (Toms.NoteOn) {
			Toms.NoteOn = false;
		}
		if (Sample1.NoteOn) {
			Sample1.NoteOn = false;
		}
		if (Sample1.NoteOnAlt) {
			Sample1.NoteOnAlt = false;
		}
		if (Sample2.NoteOn) {
			Sample2.NoteOn = false;
		}
		if (Sample2.NoteOnAlt) {
			Sample2.NoteOnAlt = false;
		}
	}
    private void OnDestroy()
    {
        if (InputDevice != null)
        {
            InputDevice.StopReceiving();
            InputDevice.Close();
        }
    }

	void RouteNoteOn(NoteOnMessage noteOnMessage) {
		Channel channel = noteOnMessage.Channel;
		Pitch pitch = noteOnMessage.Pitch;
		int velocity = noteOnMessage.Velocity;

		if (velocity == 0) return;

		Threading.RunOnMain(() => {
			if (channel == DeviceChannel) {
				switch(pitch) {
					// Kick
					case Pitch.C4:
						Kick.NoteOn = true;
						break;
					// Snare
					case Pitch.CSharp4:
						Snare.NoteOn = true;
						break;
					// Rimshot
					case Pitch.D4:
						Rimshot.NoteOn = true;
						break;
					// Clap
					case Pitch.DSharp4:
						Clap.NoteOn = true;
						break;
					// Toms
					case Pitch.E4:
						Toms.NoteOn = true;
						break;
					// Sample1
					case Pitch.F4:
						Sample1.NoteOn = true;
						break;
					// Sample1 alt
					case Pitch.FSharp4: 	
						Sample1.NoteOnAlt = true;
						break;
					// Sample2
					case Pitch.G4:
						Sample2.NoteOn = true;
						break;
					// Sample2 alt
					case Pitch.GSharp4: 	
						Sample2.NoteOnAlt = true;
						break;
				}
			}
		});
	}

	void RouteControlChange(ControlChangeMessage controlChangeMessage) {
		Channel channel = controlChangeMessage.Channel;
		int control = controlChangeMessage.Control.Number();
		float value = AsFloat(controlChangeMessage.Value);

		Threading.RunOnMain(() => {
			if (channel == DeviceChannel) {
				switch(control) {
					case 2:
						Kick.Attack = value;
						break;
					case 5:
						Kick.Decay = value;
						break;
					case 65:
						Kick.Pitch = value;
						break;
					case 3:
						Kick.Tune = value;
						break;
					case 4:
						Kick.Noise = value;
						break;
					case 67:
						Snare.NoiseDecay = value;
						break;
					case 13:
						Snare.Noise = value;
						break;
					case 11:
						Snare.Tune = value;
						break;
					case 18:
						Clap.Filter = value;
						break;
					case 75:
						Clap.Decay = value;
						break;
					case 79:
						Toms.Attack = value;
						break;
					case 20:
						Toms.Decay = value;
						break;
					case 82:
						Toms.Pitch = value;
						break;
					case 19:
						Toms.Tune = value;
						break;
					case 84:
						Sample1.Tune = value;
						break;
					case 85:
						Sample1.Decay = value;
						break;
					case 89:
						Sample2.Tune = value;
						break;
					case 90:
						Sample2.Decay = value;
						break;
				}
			}
		});
	}

	float AsFloat(int midiInt) {
		return ((float)midiInt/127f);
	}
}
