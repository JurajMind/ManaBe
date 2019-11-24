using smartHookah.Controllers;
using System.Threading.Tasks;

namespace smartHookah.Mappers.ViewModelMappers.Person
{
    public interface IPersonIndexViewModelMapper
    {
        Task<PersonIndexViewModel> Map(Models.Db.Person person);
    }
}