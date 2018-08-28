using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StockDatabase.Repositories.Interfaces;

namespace StockDatabase.Hubs {
    public class StockHub : Hub {
        private readonly IUnitOfWork _unitOfWork;

        public StockHub (IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork ??
                throw new System.ArgumentNullException (nameof (unitOfWork));
        }
    }

}