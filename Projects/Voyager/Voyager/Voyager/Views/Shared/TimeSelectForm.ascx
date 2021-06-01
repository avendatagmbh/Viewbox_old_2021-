<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

                    <div id="select_mode" class="date_form">
						<span>zeitliche Begrenzungen</span>
						<div>
							<input type="radio" name="mode" value="ab_7Uhr" checked ="checked"/>
							ab 7Uhr
						</div>
						<div>
							<input type="radio" name="mode" value="ab_8Uhr"/>
							ab 8Uhr
						</div>
						<div>
							<input type="radio" name="mode" value="Vorabend"/>
							Vorabend
						</div>
						<button id="select_ok" type="button" onclick="select_Ok(this)">Ok</button>
						<button name="date_cancel" type="button" onclick ="select_Cancel()">Abbrechen</button>
					</div>