﻿// Released into the public domain.
// http://creativecommons.org/publicdomain/zero/1.0/

namespace StopPrintJobs
{
    partial class StopPrintJobsService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TheEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.TheEventLog)).BeginInit();
            // 
            // StopPrintJobsService
            // 
            this.ServiceName = "StopPrintJobs";
            ((System.ComponentModel.ISupportInitialize)(this.TheEventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog TheEventLog;
    }
}
