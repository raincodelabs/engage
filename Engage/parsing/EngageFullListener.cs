using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.parsing
{
	public class EngageFullListener : EngageBaseListener
	{
		public NC.EngSpec Root;

		public override void EnterEngSpec(EngageParser.EngSpecContext context)
		{
			Root = new NC.EngSpec();
		}

		public override void ExitEngSpec(EngageParser.EngSpecContext context)
		{
			Root.NS = context.ID().GetText();
			foreach (var typ in context.typeDecl())
				Root.Types.Add(ToTypeDecl(typ));
			foreach (var token in context.tokenDecl())
				Root.Tokens.Add(ToTokenDecl(token));
			foreach (var handler in context.handlerDecl())
				Root.Handlers.Add(ToHandlerDecl(handler));
		}

		private NC.TypeDecl ToTypeDecl(EngageParser.TypeDeclContext typ)
		{
			var td = new NC.TypeDecl();
			if (typ.superType() != null)
				td.Super = typ.superType().ID().GetText();
			foreach (var id in typ.ID())
				td.Names.Add(id.GetText());
			return td;
		}

		private NC.TokenDecl ToTokenDecl(EngageParser.TokenDeclContext token)
		{
			var td = new NC.TokenDecl();
			foreach (var lex in token.lexeme())
			{
				if (lex.Q != null)
					td.Names.Add(new NC.LiteralLex(Unquote(lex.Q.Text)));
				else if (lex.N != null)
					td.Names.Add(new NC.NumberLex {Special = true});

				if (lex.S != null)
					td.Names.Add(new NC.StringLex {Special = true});
			}

			td.Type = token.ID().GetText();

			return td;
		}

		private NC.HandlerDecl ToHandlerDecl(EngageParser.HandlerDeclContext handler)
		{
			var hd = new NC.HandlerDecl();
			hd.LHS = ToTrigger(handler.trigger());
			hd.RHS = ToReaction(handler.reaction());
			if (handler.Adv != null)
			{
				hd.Context.AddRange(ToContext(handler.assignment()));
				switch (handler.Adv.Text)
				{
					case "where":
						hd.ComboType = NC.ComboEnum.Where;
						break;
					case "while":
						hd.ComboType = NC.ComboEnum.While;
						break;
					default:
						Console.WriteLine("[x] Unknown combo type in handler: " + handler.Adv.Text);
						return null;
				}
			}

			return hd;
		}

		private NC.Trigger ToTrigger(EngageParser.TriggerContext trigger)
		{
			NC.Trigger result = null;
			if (trigger.T != null)
				result = new NC.Trigger {Terminal = Unquote(trigger.T.Text)};
			else if (trigger.Bof != null)
				result = new NC.Trigger {Special = NC.SpecialTrigger.BOF};
			else if (trigger.Eof != null)
				result = new NC.Trigger {Special = NC.SpecialTrigger.EOF};
			else if (trigger.NT != null)
				result = new NC.Trigger {NonTerminal = trigger.NT.Text};

			if (result != null && trigger.Flag != null)
				result.Flag = trigger.Flag.Text;
			return result;
		}

		private NC.Reaction ToReaction(EngageParser.ReactionContext reaction)
		{
			switch (reaction.Command.Text)
			{
				case "push":
					var p = new NC.PushReaction {Name = reaction.name().ID().GetText()};
					foreach (var id in reaction.ID())
						p.Args.Add(id.GetText());
					return p;
				case "wrap":
					var w = new NC.WrapReaction {Name = reaction.name().ID().GetText()};
					foreach (var id in reaction.ID())
						w.Args.Add(id.GetText());
					return w;
				case "lift":
					return new NC.LiftReaction {Flag = reaction.flag().GetText()};
				case "drop":
					return new NC.DropReaction {Flag = reaction.flag().GetText()};
				case "trim":
					return new NC.TrimReaction {Name = reaction.name().ID().GetText(), Starred = reaction.Star != null};
				case "pass":
					return new NC.PassReaction();
				case "dump":
					return new NC.DumpReaction(reaction.name()?.ID()?.GetText());
				default:
					return null;
			}
		}

		private IEnumerable<NC.Assignment> ToContext(EngageParser.AssignmentContext[] assignment)
			=> assignment
				.Select(assignmentContext => new NC.Assignment
				{
					LHS = assignmentContext.ID()?.GetText(),
					RHS = ToOperation(assignmentContext.operation())
				});

		private NC.Reaction ToOperation(EngageParser.OperationContext operation)
		{
			switch (operation.Command.Text)
			{
				case "pop":
					return new NC.PopAction {Name = operation.name().GetText()};
				case "pop*":
					return new NC.PopStarAction {Name = operation.name().GetText()};
				case "await":
					var a = new NC.AwaitAction();
					a.Name = operation.name().GetText();
					if (operation.ExtraContext != null)
						a.ExtraContext = operation.ExtraContext.Text;
					if (operation.LocalContext != null)
						a.TmpContext = operation.LocalContext.Text;
					return a;
				case "await*":
					var s = new NC.AwaitStarAction();
					s.Name = operation.name().GetText();
					if (operation.LocalContext != null)
						s.TmpContext = operation.LocalContext.Text;
					return s;
				case "tear":
					return new NC.TearAction {Name = operation.name().GetText()};
				case "dump":
					return new NC.DumpReaction(operation.name()?.ID().GetText());
				default:
					return null;
			}
		}

		private string Unquote(string value)
		{
			if (value != null && value.Length > 1 && value[0] == '\'' && value[^1] == '\'')
				return value.Substring(1, value.Length - 2);
			else
				return value;
		}
	}
}