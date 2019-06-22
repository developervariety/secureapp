<?php

namespace App\Model;

use Awobaz\Compoships\Database\Eloquent\Model;

class Update extends Model
{
    protected $table = "updates";
    protected $fillable = [
        "application",
        "version",
        "storedLocation",
        "hash"
    ];
}