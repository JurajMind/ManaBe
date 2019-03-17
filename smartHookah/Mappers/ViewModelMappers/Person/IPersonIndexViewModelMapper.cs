using System.Threading.Tasks;
using smartHookah.Controllers;

namespace smartHookah.Mappers.ViewModelMappers.Person
{
    public interface IPersonIndexViewModelMapper
    {
        Task<PersonIndexViewModel> Map(Models.Db.Person person);
    }
}