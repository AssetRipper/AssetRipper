# Running on mac
In order to run Asset Ripper on mac, you'll first need to have everything listed in the [requirements](https://assetripper.github.io/AssetRipper/articles/Requirements.html) tab.

## Running the GUI version
If you're done with the requirements, lets see how to run the GUI version first.

First, you'll need to download the GUI release from [here](https://github.com/AssetRipper/AssetRipper/releases).

Make sure you download the `AssetRipperGUI_mac64.zip` file as shown below.

![Screenshot 2022-03-05 at 10 21 27 AM](https://user-images.githubusercontent.com/88390577/156868451-6fc2a979-8a87-4704-99bb-8bc2df48c9ca.png)

After downloading, right click and open the file to extract as shown below.

![Screenshot 2022-03-05 at 10 25 14 AM](https://user-images.githubusercontent.com/88390577/156868582-2d867304-9681-41d4-8d72-cb4c47d1b1af.png)

After extracting, you should see this folder named `AssetRipperGUI_mac64`.

![Screenshot 2022-03-05 at 10 26 34 AM](https://user-images.githubusercontent.com/88390577/156868626-533bccef-37ea-4c5b-ad80-3341f8f3f454.png)

And inside that folder, there should be many other files:

![Screenshot 2022-03-05 at 10 27 29 AM](https://user-images.githubusercontent.com/88390577/156868651-5283029b-e689-4df7-8048-47c183b8d599.png)

You should also see this file called `AssetRipper`.

![Screenshot 2022-03-05 at 10 29 44 AM](https://user-images.githubusercontent.com/88390577/156868715-41e815bb-88bc-4380-a91c-551387e08baa.png)

Now go back to the `AssetRipperGUI_mac64` folder and select it. Then go to `Finder`, `Services`, `New Terminal at Folder` as shown.

![Screenshot 2022-03-05 at 10 36 10 AM](https://user-images.githubusercontent.com/88390577/156868874-8b17a0ca-43bc-46ef-8437-8c876390cbf9.png)

Now you should see the terminal open:

![Screenshot 2022-03-05 at 10 39 01 AM](https://user-images.githubusercontent.com/88390577/156868950-83b2768a-0cbd-4be8-8075-03f1c3f7389d.png)

Now you'll need to type in the following command and press `enter`:

```
./AssetRipper
```

Then you should see an error saying `-bash: ./AssetRipper: Permission denied`.

![Screenshot 2022-03-05 at 10 43 21 AM](https://user-images.githubusercontent.com/88390577/156869087-7f44c5d5-a905-48c1-9767-1177ea80636f.png)

To fix that, you'll need to type in this command:

```
chmod +x AssetRipper
```
After entering that, you should see an empty line

![Screenshot 2022-03-05 at 10 45 34 AM](https://user-images.githubusercontent.com/88390577/156869157-cf8d8df5-20dc-4875-a68f-5229b83604af.png)

Now again try the following command and it should work.

```
./AssetRipper
```

Now something like this should pop up:

![Screenshot 2022-03-05 at 10 48 08 AM](https://user-images.githubusercontent.com/88390577/156869228-c57834f6-aa6a-4152-9f55-9aca17257469.png)

Click cancel. To verify that, you'll need to open system preferences using `⌘ + space` and search for `System preferences` and press `enter`. Then inside, find for `Security & Privacy` as shown below:

![Screenshot 2022-03-05 at 10 51 10 AM](https://user-images.githubusercontent.com/88390577/156869354-765a517c-8eac-4ccb-9c41-9958ac373749.png)

Now go to the `General` tab:

![Screenshot 2022-03-05 at 10 53 12 AM](https://user-images.githubusercontent.com/88390577/156869404-8b861e79-2cdf-461b-a7e5-2c9e1731455c.png)

And at the bottom, something like this should popup. Click Allow Anyway:

![Screenshot 2022-03-05 at 10 53 53 AM](https://user-images.githubusercontent.com/88390577/156869420-160157c2-da4c-4120-b19c-06cfff1f8228.png)

Now again try this command:

```
./AssetRipper
```

Then it will show somthing like this, click Open:

![Screenshot 2022-03-05 at 10 56 29 AM](https://user-images.githubusercontent.com/88390577/156869470-2c49847e-528c-4735-a328-cb1d3d7736a7.png)

Then things like this will popup, click OK and do the same steps shown above to open them:

![Screenshot 2022-03-05 at 10 56 55 AM](https://user-images.githubusercontent.com/88390577/156869487-3fbfc3f9-aed6-4ce4-9b87-869662a6aab2.png)

![Screenshot 2022-03-05 at 10 58 21 AM](https://user-images.githubusercontent.com/88390577/156869536-62fdd0a0-a049-4904-a528-dda0781cad63.png)

Like that you should see multiple popups, click OK and allow them.

After a while, you should see Asset Ripper GUI open:

![Screenshot 2022-03-05 at 11 00 27 AM](https://user-images.githubusercontent.com/88390577/156869633-0db5470c-8bdd-48ae-b940-6d3a760429ea.png)

Now lets say you got it opened, but you closed it and want to open it again. But luckily, you won't need to follow all the steps above to open again!

Just go to the `AssetRipperGUI_mac64` folder and double click this file and it should open again!

![Screenshot 2022-03-05 at 11 02 44 AM](https://user-images.githubusercontent.com/88390577/156869753-0acf7092-62d3-4f57-9086-dc941c643209.png)

![Screenshot 2022-03-05 at 11 04 39 AM](https://user-images.githubusercontent.com/88390577/156869786-8ed64cba-df09-4d21-a647-cfb2bf3ea990.png)
