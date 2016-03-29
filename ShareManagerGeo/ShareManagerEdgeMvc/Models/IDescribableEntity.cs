using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareManagerEdgeMvc.Models
{
    interface IDescribableEntity
    {
        // Override this method to provide a description of the entity for audit purposes
        string Describe();
    }
}
