# VrPet

## How to contribute and run this project
This readme file will go over the following steps in details: 
* Installing github and cloning the repository
* Setting up the environment
* Adding stuff to the project using git

#### Installing github and cloning the repository

1. First make sure you have unity version 2017.2.0 It can be downloaded from the following link:
https://unity3d.com/get-unity/download/archive?_ga=2.227330845.1408249710.1511596106-485957526.1510278119 make sure you install android and IOS platforms while installing unity.
2. Download git bash from the following link https://git-scm.com/downloads when installing it just keep the default settings checked.
3. If you are reading this and somehow don't have a git username yet please create one and request access to the repository from me (Ali)
4. Go to the folder you want the repository to be in and right click and you should see an option that says **Git Bash Here** (Click it and command prompt should open)
5. Once that is open now type the following command `git clone <Repositoy Link>` The repository link can be fond in the github repository on the top right corner there is a button that says clone or download if you click it copy the link that is there. Mine would be the following : `git clone https://github.com/alihamie/VrPet.git`
6. You are all done, you have git installed and the repository will finish downloading soon.

### Setting up the environement

1. Before setting up the Unity environment we want to make sure we have the Android SDK to develop for android apps.
Go to the following link https://developer.android.com/studio/index.html. Scroll all the way to the buttom to where it says **Get just the command line tools** and download the tools that match your platform, save them somewhere on your computer and unzip them.
2. You also need the Java tools go to http://www.oracle.com/technetwork/java/javase/downloads/jdk9-downloads-3848520.html and download the one that matches your platfrom and save it somewhere on your computer that you would remmember.
3. So now open Unity and open an existing project, navigate to the project you just downloaded using the steps above and open it.
4. Once the project loads in Unity you will probably have a lot of errors and the project won't run, don't panic this is expected.
5. Now go to File --> Build Settings. On the left where there is a list of platforms select Android and press on switch platform.
6. Once that is done stay in the same window and on the right where it says **Texture compression** choose **ASTC**. Check the Development build and the script debugging checkbox.
7. Now you're done with that window. Go to Edit --> Preferences --> External Tools and scroll down to where you can see **SDK** and **JDK** , for the SDK press on browse and go to where you unzipped and downlaoded the android sdk and select that folder. For the JDK do the same thing. JDK stands for Java Development Kit.
8. Now go to Edit --> Project Settings --> Player. There you can find a bunch of settings, press on XR Settings and check the checkbox that says Virtual REality Supported. In the Virtual Reality SDKs press the plus button and select Oculus.
9. Finally go to where it says Other Settings and make sure Multithreaded rendering is checked and dynamic batching as well. Under Indentification where it says Minimum API Level select **Android 5.0 'Lollipop' (API Level 21)**
10. That's it now go to the following folder _VrPetAsset --> Scenes --> GameScenes and select the Living room scene. Press Play and see if it actually plays and that you don't have any errors.

### Adding stuff to the project using git
(I'll add this once everyone has there projects set and working)

# Please contact me if you have trouble with any of these steps

