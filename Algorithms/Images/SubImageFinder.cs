using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public class SubImageFinder
    {
        private readonly BImage _base;

        public bool AllOccurrences { get; set; } = false;
        public Color? ByColor { set; get; } = null;

        public Pixel FirstPos { get; private set; }
        public List<Pixel> AllPos { get; private set; }

        public SubImageFinder(BImage baseImg)
        {
            _base = baseImg;
        }

        public bool Find(BImage sub)
        {
            if (AllOccurrences)
            {
                AllPos = _base.Matches(sub, ByColor);
                var found = AllPos.Any();
                if (found)
                    FirstPos = AllPos[0];

                return found;
            }

            var found_ = _base.MatchSingle(sub, out var pos, ByColor);
            FirstPos = pos;
            return found_;
        }
    }
}
