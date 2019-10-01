﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WordCounterBot.BLL.Contracts
{
    public interface IController
    {
        Task HandleUpdate(Update update);
    }
}
