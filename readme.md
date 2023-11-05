# Unity Setup

### Manual Installation
Before you begin you will need a copy of Unity 2020.3.5:
* [For Windows](https://download.unity3d.com/download_unity/8095aa901b9b/Windows64EditorInstaller/UnitySetup64-2020.3.5f1.exe)
* [For Linux](https://download.unity3d.com/download_unity/8095aa901b9b/LinuxEditorInstaller/Unity-2020.3.5f1.tar.xz)
* [For MacOS](https://download.unity3d.com/download_unity/8095aa901b9b/MacEditorInstaller/Unity-2020.3.5f1.pkg)

 If you're on Linux or Mac you'll also want to install the Windows Build Support component. Install these after you've installed the editor from the above links.
  * [For Linux](https://download.unity3d.com/download_unity/8095aa901b9b/MacEditorTargetInstaller/UnitySetup-Windows-Mono-Support-for-Editor-2020.3.5f1.pkg)
  * [For MacOS](https://download.unity3d.com/download_unity/8095aa901b9b/MacEditorTargetInstaller/UnitySetup-Windows-Mono-Support-for-Editor-2020.3.5f1.pkg)

### Unity Hub Installation
If you have Unity Hub and would prefer to install it through there, copy and paste this link into your web browsers adress bar:

unityhub://2020.3.5f1/8095aa901b9b

Make sure to enable the **Windows Build Support** component before installing if you're running on Linux or MacOS.

# Tools Setup

Create a new Unity 2020.3.5 project and then do one of the following:

### Import Package
The easiest way to install the tools is to import the unity package.

* On the right hand side, under the **Releases** section, select a release you'd like to download.
* On the page that opens, download the **BallisticUnityTools.unitypackage** file.
* In Unity, with your project open, navigate to ``Assets -> Import Package -> Custom Package`` and then open the the **BallisticUnityTools.unitypackage** file you downloaded.

You can also redo this to update to a newer version of the tools!

### Download Repo
If you'd like to manually extract the files into your project, you can download the repository.

* At the top of this page, click the green **Code** button.
* Click **Download ZIP** in the menu that pops up.
* Navigate to the location of the Unity project on your computer. You should see a folder called **Assets**
* Open the ZIP file you downloaded and then extract the **Assets** folder inside of it into your Unity projects folder. Allow it to overwrite any files if needed.

[More information can be found here.](https://ballisticng-documentation.readthedocs.io/en/latest/unity_tools/install_update.html)
