# Relo
Read about it on the [google doc](https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit?usp=sharing)

Ask friends to email harkergamedev@gmail.com with their github account to join


## How to run
* Install monogame. Instructions for [windows](http://www.gamefromscratch.com/post/2015/06/10/Getting-Started-with-MonoGame-on-Windows.aspx) and [mac](http://www.gamefromscratch.com/post/2015/06/09/Getting-Started-with-MonoGame-on-MacOS.aspx).
* Install [git](https://git-scm.com/downloads)
* Open git bash or terminal, and type in the following:
```
git clone https://github.com/HarkerGameDev/TheGame.git
```
* Open "TheGame.sln" from the TheGame folder you just downloaded -- check your current directory from shell with `pwd`
* Hit the run button in the top toolbar of your IDE (Xamarin or Visual Studio)
* Play the game with the arrow keys and Right Shift, or check the troubleshooting section if it did not work
* More controls and multiplayer can be seen and configured inside `Source/GameData.cs`


### Troubleshooting
* *No reported issues yet. Contact harkergamedev@gmail.com to report an issue*


## TODO
* Look at the [google doc](https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit?usp=sharing)
* Note to art theme: Aim for a super-actiony mix of [a new york asthetic](http://www.newyorkwallpapershd.com/user-content/uploads/wall/o/10/New-York-Empire-State-Building-1920x1200-Wallpaper.jpg) and [Just Cause 2](https://nigmabox.files.wordpress.com/2014/01/011justcause2_2010-05-10_09-29-42-95.jpeg)

## Running on mac
* Somehow managed to fix the god-awful Mac issues based on [this link](https://github.com/mono/MonoGame/issues/3790#issuecomment-128841617)

## A note about mac
* As of the current version of the mono framework, monogame on mac cannot read the generic file types like spritefonts that the windows version uses. This means that when any content is added to the game, a built .xnb file must be added to the mac content folder. I understand this is ugly, and I hate just as much as the next person, but as of right now that is all we can do.

## Adding content
* Make a change
* *TEST IT*
* Commit and push with some useful message
* Cross it off of the TODO list (on the [google doc](https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit?usp=sharing))

## How to contribute
**Email harkergamedev@gmail.com with github username for push access**

*Note: instructions are for terminal. Use your IDE or some GUI if you like it more*

### Clone
Clone to initially copy and set up the remote repository on your computer
```
git clone https://github.com/HarkerGameDev/TheGame.git
cd TheGame
```

### Push
Commit and push to put your changes to the remote repository (on github)
```
git add .
git commit -m "Enter a descriptive message of what you did here"
git push
```

### Pull
Pull to update your local repository (files) from the remote repository (on github)
```
git pull
```

### Merge
Pushing or pulling may not work if local changes are not synced up with remote changes, so merging must be done
*TODO: add merge instructions*

### Debugging
Use a combination of `git status` and output from other commands to see what went wrong


## Making requests
### Raising an issue
Go to the issues section on the repository and create an issue
### Editing GoogleDoc
Add what you wish to be implemented to the google doc. If you want to do it, put your name under the task as well
### Email
If none of the above are desireable, email harkergamedev@gmail.com
