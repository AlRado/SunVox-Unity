using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SunVoxExample6 : MonoBehaviour {

  /*
     You can use SunVox library freely, 
     but the following text should be included in your products (e.g. in About window):

     SunVox modular synthesizer
     Copyright (c) 2008 - 2018, Alexander Zolotov <nightradio@gmail.com>, WarmPlace.ru

     Ogg Vorbis 'Tremor' integer playback codec
     Copyright (c) 2002, Xiph.org Foundation
  */

  //
  // * Creating the new Sampler and loading XI-file from memory to it.
  //

  public Text Text;

  private int sunvox_sample_size;
  private byte[] sunvox_sample;

  void Start () {
    try {
      int ver = SunVox.sv_init ("0", 44100, 2, 0);
      if (ver >= 0) {
        int major = (ver >> 16) & 255;
        int minor1 = (ver >> 8) & 255;
        int minor2 = (ver) & 255;
        log (String.Format ("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));

        SunVox.sv_open_slot (0);

        StartCoroutine (SamplerCoroutine ());

      } else {
        log ("sv_init() error " + ver);
      }

    } catch (Exception e) {
      log ("Exception: " + e);
    }
  }

  private IEnumerator SamplerCoroutine () {
    //Create Sampler module:
    SunVox.sv_lock_slot (0);
    int mod_num = SunVox.sv_new_module (0, "Sampler", "Sampler", 0, 0, 0);
    SunVox.sv_unlock_slot (0);
    if (mod_num >= 0) {
      log ("New module created: " + mod_num);
      //Connect the new module to the Main Output:
      SunVox.sv_lock_slot (0);
      SunVox.sv_connect_module (0, mod_num, 0);
      SunVox.sv_unlock_slot (0);

      //Load a sample:
      log ("Load sample and play it");
      loadBinaryAsset (getDataPath ("flute.xi"), () => StartCoroutine(onBinaryFileLoadedCoroutine(mod_num)));

      yield return new WaitForSeconds (1);

    } else {
      log ("Can't create the new module");
    }
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

      sunvox_sample = www.bytes;
      sunvox_sample_size = sunvox_sample.Length;
      if (callback != null) callback ();
    }
  }

  private IEnumerator onBinaryFileLoadedCoroutine (int mod_num) {
      SunVox.sv_sampler_load_from_memory (0, mod_num, sunvox_sample, sunvox_sample_size, -1);
      //Send Note ON:
      log ("Note ON");
      SunVox.sv_send_event (0, 0, 64, 128, mod_num + 1, 0, 0);
      yield return new WaitForSeconds (1);
      //Send Note OFF:
      log ("Note OFF");
      SunVox.sv_send_event (0, 0, 128, 128, mod_num + 1, 0, 0);
  }

  private void log (string msg) {
    Debug.Log (msg);
    Text.text = Text.text + "\n" + msg;
  }

  private void OnDestroy () {
    if (!enabled) return;

    SunVox.sv_close_slot (0);
    SunVox.sv_deinit ();
  }

}