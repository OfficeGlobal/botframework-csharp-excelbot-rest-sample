﻿/* 
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using AuthBot;
using ExcelBot.Forms;
using ExcelBot.Helpers;
using ExcelBot.Workers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace ExcelBot.Dialogs
{
    [Serializable]
    public class ConfirmOpenWorkbookDialog : IDialog<bool>
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            PromptDialog.Confirm(context, AfterConfirming_OpenWorkbook, $"I don't have a workbook open. Do you want me to open a workbook?");
        }

        private async Task AfterConfirming_OpenWorkbook(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                // Call the OpenWorkbook Form
                context.Call<OpenWorkbookForm>(
                        new FormDialog<OpenWorkbookForm>(new OpenWorkbookForm(), OpenWorkbookForm.BuildForm, FormOptions.PromptInStart),
                        OpenWorkbook_FormComplete);
            }
            else
            {
                await context.PostAsync("Okay! I will just sit tight until you tell me which workbook we should work with");
                context.Done<bool>(false);
            }
        }

        private async Task OpenWorkbook_FormComplete(IDialogContext context, IAwaitable<OpenWorkbookForm> result)
        {
            OpenWorkbookForm form = null;
            try
            {
                form = await result;
            }
            catch
            {
            }

            if (form != null)
            {
                // Get access token to see if user is authenticated
                ServicesHelper.AccessToken = await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);

                // Open the workbook
                await WorkbookWorker.DoOpenWorkbookAsync(context, form.WorkbookName);
                context.Done<bool>(true);
            }
            else
            {
                await context.PostAsync("Okay! I will just sit tight until you tell me which workbook we should work with");
                context.Done<bool>(false);
            }
        }
    }
}