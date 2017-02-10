<?php
	$sitekey = null;
	
	if(!empty($_GET['key']) && strlen($_GET['key']) > 0)
		$sitekey = $_GET['key'];
	
	if(isset($_POST['g-recaptcha-response']) && strlen($_POST['g-recaptcha-response'])>0) {
		setcookie("g-recaptcha-response", $_POST['g-recaptcha-response'], time() + 120);
	}
?>
<html>
  <head>
    <title>reCAPTCHA</title>
	 <meta http-equiv="X-UA-Compatible" content="IE=edge" />
	 <meta http-equiv="Content-Language" content="fr-FR">
     <?php if($sitekey == null) { echo 'sitekey not found'; return; } else echo '<script src="https://www.google.com/recaptcha/api.js"></script>';?>
  </head>
  <body>
    <form action="<?php echo '?key='.$sitekey;?>" method="POST">
      <div class="g-recaptcha" data-sitekey="<?php echo $sitekey; ?>"></div>
      <br/>
      <input type="submit" value="Submit">
    </form>
  </body>
</html>