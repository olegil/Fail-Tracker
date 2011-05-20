﻿using System;
using System.Linq;
using System.Web.Mvc;
using FailTracker.Core.Data;
using FailTracker.Core.Domain;
using FailTracker.Web.Models.Issues;
using Microsoft.Web.Mvc;

namespace FailTracker.Web.Controllers
{
	public class IssuesController : Controller
	{
		private readonly IRepository<Issue> _issues;
		private readonly IRepository<User> _users;

		public IssuesController(IRepository<Issue> issues, IRepository<User> users)
		{
			_issues = issues;
			_users = users;
		}

		public ActionResult Dashboard()
		{
			var issues = (from i in _issues.Query()
						  let assignedTo = i.AssignedTo != null ? i.AssignedTo.EmailAddress : null
			              select new IssueViewModel
			                     	{
			                     		ID = i.ID, 
										Title = i.Title, 
										AssignedTo = assignedTo,
										Size = i.Size,
										Type = i.Type
			                     	}).ToArray();

			return View(issues);
		}

		[HttpGet]
		public ActionResult AddIssue()
		{
			return View();
		}

		[HttpPost]
		public ActionResult AddIssue(AddIssueForm form)
		{
			//TODO: Abstract this down/out
			var currentUser = _users.Query().Single(u => u.EmailAddress == User.Identity.Name);
			var issue = Issue.CreateNewStory(form.Title, currentUser, form.Body);
			issue.ChangeTypeTo(form.Type);
			issue.SetSizeTo(form.Size);

			if (form.AssignedTo.HasValue)
			{
				issue.ReassignTo(_users.Query().Where(u => u.ID == form.AssignedTo).Single());
			}

			_issues.Save(issue);

			return this.RedirectToAction(c => c.View(issue.ID));
		}

		public ActionResult View(Guid id)
		{
			var model = (from i in _issues.Query()
			             where i.ID == id
						 let assignedTo = i.AssignedTo != null ? i.AssignedTo.EmailAddress : null
						 select new ViewIssueViewModel
			                    	{
										Title = i.Title,
										CreatedBy = i.CreatedBy.EmailAddress,
										AssignedTo = assignedTo,
										Body = i.Body,
										Size = i.Size,
										Type = i.Type
			                    	}).Single();

			return View(model);
		}
	}
}
