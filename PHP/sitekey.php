<?php
	$sitekey = null;
	
	if(!empty($_GET['key']) && strlen($_GET['key']) > 0)
		$sitekey = $_GET['key'];
	
	if(isset($_POST['g-recaptcha-response']) && strlen($_POST['g-recaptcha-response'])>0) {
		$expire_time = time() + 120;
		setcookie("g-recaptcha-response", $_POST['g-recaptcha-response'], $expire_time);
		$file = fopen('recaptcha_response.txt', 'w');
		fwrite($file, $expire_time. "\n" . $_POST['g-recaptcha-response']);
		fclose($file);
	}
?>
<html>
  <head>
  	<meta charset="UTF-8">
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>reCAPTCHA</title>
     <?php if($sitekey == null) { echo 'sitekey not found'; return; } else echo '<script type="text/javascript" src="https://www.google.com/recaptcha/api.js"></script>';?>
  </head>
  <body>
    <form action="<?php echo '?key='.$sitekey;?>" method="POST">
      <div class="g-recaptcha" data-sitekey="<?php echo $sitekey; ?>"></div>
      <br/>
      <input type="submit" value="Submit">
    </form>
  </body>
</html>