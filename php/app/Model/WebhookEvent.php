<?php

namespace App\Model;

use Awobaz\Compoships\Database\Eloquent\Model;

class WebhookEvent extends Model
{
    protected $table = "events";
    public $timestamps = false;

    protected $fillable = [
        "applicationID",
        "initialized",
        "userApproved",
        "userDeclined",
        "serialActivated",
        "serialDeclined",
        "exception",
        "webhook"
    ];
}