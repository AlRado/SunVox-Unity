using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SunVoxExample2 : MonoBehaviour {
  /*
     You can use SunVox library freely, 
     but the following text should be included in your products (e.g. in About window):

     SunVox modular synthesizer
     Copyright (c) 2008 - 2017, Alexander Zolotov <nightradio@gmail.com>, WarmPlace.ru

     Ogg Vorbis 'Tremor' integer playback codec
     Copyright (c) 2002, Xiph.org Foundation
  */

  //
  // * Loading SunVox song from memory.
  // * Playing SunVox song.
  // * Stop/Play song.
  //

  public Text Text;

  private int sunvox_song_size;
  private byte[] sunvox_song;

  private void Start () {
    log ("-Press Space for toggle music-\n");

    try {
      int ver = SunVox.sv_init ("0", 44100, 2, 0);
      if (ver >= 0) {
        int major = (ver >> 16) & 255;
        int minor1 = (ver >> 8) & 255;
        int minor2 = (ver) & 255;
        log (String.Format ("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));

        SunVox.sv_open_slot (0);

        log ("Loading SunVox song from memory...");
        loadBinaryAsset (getDataPath ("test.sunvox"), onBinaryFileLoaded);

      } else {
        log ("sv_init() error " + ver);
      }

    } catch (Exception e) {
      log ("Exception: " + e);
    }
  }

  private void onBinaryFileLoaded () {
    var music = SunVox.sv_load_from_memory (0, sunvox_song, sunvox_song_size);

    if (music >= 0) {
      log ("Loaded.");
    } else {
      log ("Load error.");
    }

    SunVox.sv_volume (0, 256);

    var songName = Marshal.PtrToStringAnsi (SunVox.sv_get_song_name (0));
    log ("song_name: " + songName);

    SunVox.sv_play_from_beginning (0);
  }

  private void log (string msg) {
    Debug.Log (msg);
    Text.text = Text.text + "\n" + msg;
  }

  private string getDataPath (string fileName) {
    var streamingAssetsPath = "";
#if UNITY_IPHONE
    streamingAssetsPath = Application.dataPath + "/Raw";
#endif

#if UNITY_ANDROID
    streamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets";
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
    streamingAssetsPath = Application.dataPath + "/StreamingAssets";
#endif

    var path = Path.Combine (streamingAssetsPath, fileName);
    log ("getDataPath: " + path);
    return path;
  }

  private void loadBinaryAsset (string path, Action callback) {
    StartCoroutine (loadBinaryAssetCoroutine (path, callback));
  }

  private IEnumerator loadBinaryAssetCoroutine (string path, Action callback) {
    using (WWW www = new WWW (path)) {
      yield return www;

      if (!string.IsNullOrEmpty (www.error)) log (www.error);

      sunvox_song = www.bytes;
      sunvox_song_size = sunvox_song.Length;
      if (callback != null) callback ();
    }
  }

  private void Update () {
    if (Input.GetKeyDown (KeyCode.Space)) {
      if (SunVox.sv_end_of_song (0) == 1) {
        SunVox.sv_play (0);
      } else {
        SunVox.sv_stop (0);
      }
    }
  }

  private void OnDestroy () {
    if (!enabled) return;

    SunVox.sv_close_slot (0);
    SunVox.sv_deinit ();
  }

}