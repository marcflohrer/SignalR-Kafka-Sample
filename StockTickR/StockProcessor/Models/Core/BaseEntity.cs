using System.ComponentModel.DataAnnotations.Schema;

namespace StockProcessor.Models.Core {
    public abstract class BaseEntity<TId> {

        [ForeignKey ("Id")]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public TId Id {
            get;
            set;
        }

        public override bool Equals (object obj) {
            var entity = obj as BaseEntity<TId>;
            return entity != null &&
                Id.Equals (entity.Id);
        }

        public override int GetHashCode () {
            return base.GetHashCode ();
        }

        public override string ToString () {
            return base.ToString ();
        }
    }
}