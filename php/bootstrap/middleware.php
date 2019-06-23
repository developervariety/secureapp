<?php

use App\Middleware\ValidationErrors;

$app->add(new ValidationErrors($container));