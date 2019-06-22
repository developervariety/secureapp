<?php

namespace App\Model;

use Awobaz\Compoships\Database\Eloquent\Model;

class Exception extends Model
{
    protected $table = "exceptions";
    protected $fillable = [
        "application",
        "version",
        "name",
        "message",
        "source",
        "stackTrace"
    ];
}