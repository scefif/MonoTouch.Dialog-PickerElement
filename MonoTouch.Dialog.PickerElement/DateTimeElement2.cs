using System;
using MonoTouch.Dialog;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ClanceysLib;
using MonoTouch.ObjCRuntime;

namespace MonoTouch.Dialog.PickerElement
{
	public class DateTimeElement2 : ReadOnlyStringElement {
		public DateTime DateValue;
		public UIDatePickerMode Mode { 
			get {
				return datePicker.Mode;
			}
			set{
				datePicker.Mode = value;
			}
		}
			
		public UIView ViewForPicker; 
		public UIDatePicker datePicker;
		public event Action<DateTimeElement2> DateSelected;
		public event EventHandler PickerClosed;
		public event EventHandler PickerShown;
				
		private DialogViewController Dvc;
		private UIButton closeBtn;
		private UIBarButtonItem oldRightBtn;
		private UIBarButtonItem doneButton;
		private bool wiredStarted = false;			
		
		
		protected internal NSDateFormatter fmt = new NSDateFormatter () {
			DateStyle = NSDateFormatterStyle.Short
		};
		
		public DateTimeElement2 (string caption, DateTime date) : base (caption)
		{			
			DateValue = date;
			
			// create picker elements
			datePicker = CreatePicker ();
			datePicker.Mode = UIDatePickerMode.DateAndTime; 
			datePicker.ValueChanged += delegate {
				DateValue = datePicker.Date;				
				Value = FormatDate(DateValue);
				
				if (DateSelected != null)
					DateSelected (this);								
			};		
						
			//datePicker.Frame = PickerFrameWithSize (datePicker.SizeThatFits (SizeF.Empty));					
			closeBtn = new UIButton(new RectangleF(0,0,31,32));
			closeBtn.SetImage(UIImage.FromFile("Images/closebox.png"),UIControlState.Normal);
			closeBtn.TouchDown += delegate {
				HidePicker();
			};			
			datePicker.AddSubview(closeBtn);			
						
			Value = FormatDate (date);			
		}	
		
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Element root = Parent;
			while (root.Parent != null) {
				root = root.Parent;
			}
			
			// get rid of keyboard if another element triggered it.
			ResignFirstResponders((RootElement)root);
			
			Dvc = dvc;
			ViewForPicker = ViewForPicker ?? tableView.Superview;
			base.Selected (dvc, tableView, path);
			
			ShowPicker();			
			
			//ComboBox.ShowPicker();
			if(dvc.NavigationItem.RightBarButtonItem != doneButton)
				oldRightBtn = dvc.NavigationItem.RightBarButtonItem;
			if(doneButton == null)
				doneButton = new UIBarButtonItem("Done",UIBarButtonItemStyle.Bordered, delegate{
					HidePicker();						
				});
			dvc.NavigationItem.RightBarButtonItem = doneButton;
			if (!wiredStarted) {
				foreach(var sect in (root as RootElement)) {
					foreach(var e in sect.Elements) {
						var ee = e as EntryElement;
						if (ee != null) {
							// MonoTouch.Dialog CUSTOM: Download custom MonoTouch.Dialog from here to enable hiding picker when other element is selected:
							// https://github.com/crdeutsch/MonoTouch.Dialog
							//((EntryElement)e).EntryStarted += delegate {
							//	HidePicker();
							//};
							ee.ResignFirstResponder(false);
						}
					}
				}
				wiredStarted = true;
			}
			
			
		}
		
		public void LayoutSubviews ()
		{
			//base.LayoutSubviews ();
			var parentView = ViewForPicker;
			var parentH = parentView.Frame.Height;			
			datePicker.Frame = new RectangleF(0,parentH - datePicker.Frame.Height,parentView.Frame.Size.Width,datePicker.Frame.Height);
			closeBtn.Frame = closeBtn.Frame.SetLocation(new PointF(datePicker.Bounds.Width - 32,datePicker.Bounds.Y));
			datePicker.BringSubviewToFront(closeBtn);
		}
		
		public override void Deselected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Dvc = dvc;
			base.Deselected (dvc, tableView, path);
			///ComboBox.HidePicker();
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = FormatDate (DateValue);
			var cell = base.GetCell (tv);
			//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			return cell;				
		}
		
		public void ShowPicker()
		{
			if(PickerShown != null)
				PickerShown(this,null);
			
			LayoutSubviews ();
			datePicker.BringSubviewToFront(closeBtn);
			var parentView = ViewForPicker;
			var parentFrame = parentView.Frame;
			
			datePicker.Frame = datePicker.Frame.SetLocation(new PointF(0,parentFrame.Height));
			
			UIView.BeginAnimations("slidePickerIn");			
			UIView.SetAnimationDuration(0.3);
			UIView.SetAnimationDelegate(parentView);
			UIView.SetAnimationDidStopSelector (new Selector ("fadeInDidFinish"));
			//parentView.AddSubview(closeView);			
			parentView.AddSubview(datePicker);
						
			datePicker.Frame = datePicker.Frame.SetLocation(new PointF(0,parentFrame.Height - datePicker.Frame.Height));
			UIView.CommitAnimations();			
		}
		
		public void HidePicker() {
			if(PickerClosed!=null)
				PickerClosed(this,null);
			
			var parentView = ViewForPicker;
			
			if (parentView != null) {
				var parentH = parentView.Frame.Height;
				
				UIView.BeginAnimations("slidePickerOut");			
				UIView.SetAnimationDuration(0.3);
				UIView.SetAnimationDelegate(parentView);			
				UIView.SetAnimationDidStopSelector (new Selector ("fadeOutDidFinish"));
				datePicker.Frame = datePicker.Frame.SetLocation(new PointF(0,parentH));
				UIView.CommitAnimations();
				
				//datePicker.RemoveFromSuperview();
				
				if (Dvc != null) {
					Dvc.NavigationItem.RightBarButtonItem = oldRightBtn;
				}
			}
		}
		
		private void ResignFirstResponders(RootElement root) {
			foreach(var sect in root) {
				foreach(var e in sect.Elements) {
					var ee = e as EntryElement;
					if (ee != null) {
						ee.ResignFirstResponder(false);
					}
					var pe = e as PickerElement;
					if (pe != null) {
						pe.HidePicker();
					}
					var dte = e as DateTimeElement2;
					if (dte != null && dte != this) {
						dte.HidePicker();
					}
				}
			}
		}
		
		
		/*
		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = FormatDate (DateValue);
			var cell = base.GetCell (tv);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new MyViewController (this) {
				Autorotate = dvc.Autorotate
			};
			datePicker = CreatePicker ();
			datePicker.Frame = PickerFrameWithSize (datePicker.SizeThatFits (SizeF.Empty));
			                            
			vc.View.BackgroundColor = UIColor.Black;
			vc.View.AddSubview (datePicker);
			dvc.ActivateController (vc);
		}
 		*/
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (fmt != null){
					fmt.Dispose ();
					fmt = null;
				}
				if (datePicker != null){
					datePicker.Dispose ();
					datePicker = null;
				}
				if (closeBtn != null) {
					closeBtn.Dispose();
					closeBtn = null;
				}
			}
		}
		
		public virtual string FormatDate (DateTime dt)
		{
			switch (datePicker.Mode) {
				case UIDatePickerMode.Date:
					return fmt.ToString (dt);
				
				case UIDatePickerMode.Time:
					return dt.ToLocalTime ().ToShortTimeString ();
				
				default:
					return fmt.ToString (dt) + " " + dt.ToLocalTime ().ToShortTimeString ();
			}			
		}
		
		public virtual UIDatePicker CreatePicker ()
		{
			var picker = new UIDatePicker (RectangleF.Empty){
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				Mode = UIDatePickerMode.DateAndTime,
				Date = DateValue
			};
			return picker;
		}
		                                                                                                                                
		static RectangleF PickerFrameWithSize (SizeF size)
		{                                                                                                                                    
			var screenRect = UIScreen.MainScreen.ApplicationFrame;
			float fY = 0, fX = 0;
			
			switch (UIApplication.SharedApplication.StatusBarOrientation){
			case UIInterfaceOrientation.LandscapeLeft:
			case UIInterfaceOrientation.LandscapeRight:
				fX = (screenRect.Height - size.Width) /2;
				fY = (screenRect.Width - size.Height) / 2 -17;
				break;
				
			case UIInterfaceOrientation.Portrait:
			case UIInterfaceOrientation.PortraitUpsideDown:
				fX = (screenRect.Width - size.Width) / 2;
				fY = (screenRect.Height - size.Height) / 2 - 25;
				break;
			}
			
			return new RectangleF (fX, fY, size.Width, size.Height);
		}                                                                                                                                    
		
		
		
	}
}
