# How to use
Either compile from the source code, or download the zip file from the releases section.

Log into chrome with your epic games account, and thats it. Just make sure that you are logged into epic games on chrome. <br />
Run the exe, it will automatically get the data and run the api in headless mode.

This may seem obvious, but: YOU NEED TO BUY THE GAME!


# Requirments

### Python
Python 3 installed, should be a PATH variable (learn how to here: https://www.youtube.com/watch?v=Y2q_b4ugPWk) <br />
Install the required moduled by running the requirments file. Run `pip install -r requirements.txt`. <br>
A requirments.txt file is present in the release, so make sure to set the requirments.txt path to that.

### Chromedriver
Add chromedriver into your PATH variables. Make sure it is the right version of chromedriver. <br />
You can learn how to add to PATH here: https://www.youtube.com/watch?v=dz59GsdvUF8<br>
MAKE SURE to download the version that your broswer is currently on (check on chrome://version/).

### Other
Make sure that you are logged into epic games on chrome.


# Not working?
This most likely happens because of an outdated webdriver. <br />
Go to https://chromedriver.chromium.org/, and download version that your broswer is currently on (can check on chrome://version/). <br />
Download the corresponding variable from here, then make sure to update your chromedriver in your PATH variable.


# Something to keep in your mind
The code is total spaghetti. I did this just as a side thing to get my rewards claimed automatically, but then thought I should share it here. 
