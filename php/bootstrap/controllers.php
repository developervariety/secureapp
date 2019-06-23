<?php

use App\Controller\Authentication;
use App\Validation\Validator;

$container["validator"] = function () {
    return new Validator();
};


$container["Authentication"] = function ($container) {
    return new Authentication($container);
};