# 3s_atc
BETA : Adidas bot that support splash pages(and hmac)<br />
Please note that this is only a BETA<br />
This program uses a headless browser named PhantomJS to stay on the splash page/login on Adidas and so reduce RAM usage<br />

# How to install
Just like Solemartyr's PHP script, setup Wamp, go to your hosts file and add this line : "127.0.0.1	dev.adidas.com", then go to your /www/ folder and put the 'sitekey.php' file in it.This is needed for captcha solving.<br />
Compile the application or if you can't just run '3s_atc.exe' located in the /Release/ folder<br />

# How to use
Go to settings and select a locale <br />
Fill in the fields, put a sitekey(check 'captcha')/client id(check 'client id')/duplicate(check 'duplicate')/cookies only if the add-to-cart process needs one<br />
Select your sizes by preference, the application will cart the first available size in the list<br />
Press Run button<br />
Let the application Run and if you checked the 'captcha' box, wait until the profile's status asks you to solve captcha, then just right click on the profile and press "Solve captcha"<br />

# How to use the 'splash page' mode
Put only the product ID, your adidas account's username and password, splash page url and check 'splash page' box.<br />
Go to settings and add as many proxies you want but don't forget that more proxies = more RAM usage<br />
Press 'Run' button<br />
The proxies will connect to the splash page and wait until they pass the splash page<br />
Once a proxy reached the product page, go back to the main menu and wait until the application asks you to solve captcha<br />
The application will save the product page source code in the application folder so you can share it with the community<br />

# What if the the application crashes or bugs after a proxy has reached the product page?
The applications will save the HMAC cookie, sitekey for each proxy in a new .txt file located in the application folder named 'hmacs.txt'<br />

# Credits/Special thanks to :
-SOLEMARTYR for starting all this <br />
