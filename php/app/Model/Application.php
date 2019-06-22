<?php

namespace App\Model;

use Awobaz\Compoships\Database\Eloquent\Model;

class Application extends Model
{
    protected $table = "applications";
    protected $fillable = [
        "developer",
        "token",
        "secret",
        "name"
    ];
}