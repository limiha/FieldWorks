// Copyright (c) 2015-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using LanguageExplorer.Controls.DetailControls;
using SIL.LCModel;

namespace LanguageExplorer.Areas.Lexicon.Tools.Edit
{
	/// <summary>
	/// Summary description for LexReferencePairView.
	/// </summary>
	internal sealed class LexReferencePairView : AtomicReferenceView
	{
		/// <summary />
		private ICmObject m_displayParent;

		/// <summary />
		public LexReferencePairView() : base()
		{
		}

		/// <summary />
		public override void SetReferenceVc()
		{
			CheckDisposed();

			m_atomicReferenceVc = new LexReferencePairVc(m_cache, m_rootFlid, m_displayNameProperty);
			if (m_displayParent != null)
			{
				((LexReferencePairVc)m_atomicReferenceVc).DisplayParent = m_displayParent;
			}
		}

		/// <summary />
		public ICmObject DisplayParent
		{
			set
			{
				CheckDisposed();

				m_displayParent = value;
				if (m_atomicReferenceVc != null)
				{
					((LexReferencePairVc)m_atomicReferenceVc).DisplayParent = value;
				}
			}
		}
	}
}