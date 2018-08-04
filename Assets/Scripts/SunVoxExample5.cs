using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SunVoxExample5 : MonoBehaviour {

  /*
     You can use SunVox library freely, 
     but the following text should be included in your products (e.g. in About window):

     SunVox modular synthesizer
     Copyright (c) 2008 - 2018, Alexander Zolotov <nightradio@gmail.com>, WarmPlace.ru

     Ogg Vorbis 'Tremor' integer playback codec
     Copyright (c) 2002, Xiph.org Foundation
  */

  //
  // * Creating the new module.
  // * Loading the module from memory.
  // * Connecting the module to the main Output.
  // * Sending some events to this module.
  //

  public Text Text;

  private int sunvox_module_size;
  private byte[] sunvox_module;

  void Start () {
    try {
      int ver = SunVox.sv_init ("0", 44100, 2, 0);
      if (ver >= 0) {
        int major = (ver >> 16) & 255;
        int minor1 = (ver >> 8) & 255;
        int minor2 = (ver) & 255;
        log (String.Format ("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));

        SunVox.sv_open_slot (0);

        StartCoroutine (ModuleCoroutine ());

      } else {
        log ("sv_init() error " + ver);
      }

    } catch (Exception e) {
      log ("Exception: " + e);
    }
  }

  private IEnumerator ModuleCoroutine () {
    //Create Generator module:
    SunVox.sv_lock_slot (0);
    int mod_num = SunVox.sv_new_module (0, "Generator", "Generator", 0, 0, 0);
    SunVox.sv_unlock_slot (0);
    if (mod_num >= 0) {
      log ("New module created: " + mod_num);
      //Connect the new module to the Main Output:
      SunVox.sv_lock_slot (0);
      SunVox.sv_connect_module (0, mod_num, 0);
      SunVox.sv_unlock_slot (0);
      //Send Note ON:
      log ("Note ON");
      SunVox.sv_send_event (0, 0, 64, 128, mod_num + 1, 0, 0);
      yield return new WaitForSeconds (1);
      //Send Note OFF:
      log ("Note OFF");
      SunVox.sv_send_event (0, 0, 128, 128, mod_num + 1, 0, 0);
      yield return new WaitForSeconds (1);

    } else {
      log ("Can't create the new module");
    }

    //Load module and play it:
    log ("Load module and play it");
    loadBinaryAsset (getDataPath ("organ.sunsynth"), onBinaryFileLoaded);

    yield return new WaitForSeconds (1);
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

      sunvox_module = www.bytes;
      sunvox_module_size = sunvox_module.Length;
      if (callback != null) callback ();
    }
  }

  private void onBinaryFileLoaded () {
    int mod_num2 = SunVox.sv_load_module_from_memory (0, sunvox_module, sunvox_module_size, 0, 0, 0);
    if (mod_num2 >= 0) {
      StartCoroutine(playModuleCoroutine(mod_num2));
    } else {
      log ("Load error.");
    }
  }

  private IEnumerator playModuleCoroutine (int mod_num2) {
      log ("Module loaded: " + mod_num2);
      //Connect the new module to the Main Output:
      SunVox.sv_lock_slot (0);
      SunVox.sv_connect_module (0, mod_num2, 0);
      SunVox.sv_unlock_slot (0);
      //Send Note ON:
      log ("Note ON");
      SunVox.sv_send_event (0, 0, 64, 128, mod_num2 + 1, 0, 0);
      yield return new WaitForSeconds (1);
      //Send Note OFF:
      log ("Note OFF");
      SunVox.sv_send_event (0, 0, 128, 128, mod_num2 + 1, 0, 0);
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