# Running AssetRipper on macOS

> ⚠️ Disclaimer: The project developer does not have access to a Mac. This guide is community-contributed and might be outdated.

This guide will help you run AssetRipper on macOS. Before proceeding, ensure you have all the necessary [requirements](https://assetripper.github.io/AssetRipper/articles/Requirements.html).

## Running the GUI Version

### Step 1: Download AssetRipper

1. Download the GUI release from the [AssetRipper downloads page](https://assetripper.github.io/AssetRipper/articles/Downloads.html).
2. Choose `AssetRipper_mac_x64.zip` or `AssetRipper_mac_arm64.zip`, depending on your computer architecture. M2 processors are arm64.

   ![Download AssetRipper](images/RunningOnMac/001.png)

### Step 2: Extract the Archive

1. Right-click the downloaded file and select "Open" to extract its contents.

   ![Extract AssetRipper](images/RunningOnMac/002.png)

2. You should now see a folder named `AssetRipper_mac_x64`.

   ![AssetRipper folder](images/RunningOnMac/003.png)

3. Inside this folder, you'll find various files, including the AssetRipper executable.

   ![AssetRipper contents](images/RunningOnMac/004.png)
   ![AssetRipper executable](images/RunningOnMac/005.png)

### Step 3: Open Terminal in the AssetRipper Folder

1. Select the `AssetRipper_mac_x64` folder.
2. Go to `Finder` > `Services` > `New Terminal at Folder`.

   ![Open Terminal](images/RunningOnMac/006.png)

### Step 4: Run AssetRipper

1. In the Terminal, enter the following command:
```
./AssetRipper.GUI.Free
```
2. You may encounter a "Permission denied" error.

![Permission denied](images/RunningOnMac/008.png)

3. To fix this, enter the following command:
```
chmod +x AssetRipper.GUI.Free
```
4. Now, try running AssetRipper again:
```
./AssetRipper.GUI.Free
```

### Step 5: Handle Security Prompts

1. You may see a security prompt. Click "Cancel" for now.

![Security prompt](images/RunningOnMac/010.png)

2. Open System Preferences (⌘ + Space, search for "System Preferences").
3. Go to "Security & Privacy" > "General" tab.
4. At the bottom, click "Allow Anyway" for AssetRipper.

![Allow AssetRipper](images/RunningOnMac/013.png)

5. Run the `./AssetRipper.GUI.Free` command again in Terminal.
6. Click "Open" when prompted.

![Open AssetRipper](images/RunningOnMac/014.png)

7. You may see additional security prompts. Click "OK" and allow them as needed.

![Additional prompts](images/RunningOnMac/015.png)
![More prompts](images/RunningOnMac/016.png)

### Step 6: AssetRipper GUI

After completing these steps, the AssetRipper GUI should open:

![AssetRipper GUI](images/RunningOnMac/017.png)

## Reopening AssetRipper

Once you've gone through the initial setup, you can easily reopen AssetRipper:

1. Navigate to the `AssetRipper_mac_x64` folder.
2. Double-click the `AssetRipper.GUI.Free` file to launch the application.

![Reopen AssetRipper](images/RunningOnMac/018.png)

Congratulations! You've successfully set up and run AssetRipper on macOS.

![AssetRipper running](images/RunningOnMac/019.png)