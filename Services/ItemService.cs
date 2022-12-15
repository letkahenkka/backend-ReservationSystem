using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationSystem.Models;
using ReservationSystem.Repositories;

namespace ReservationSystem.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly IUserRepository _userRepository;

        public ItemService(IItemRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<ItemDTO> CreateItemAsync(ItemDTO dto)
        {
            Item newItem = await DTOToItem(dto);
            await _repository.AddItemAsync(newItem);
            return ItemToDTO(newItem);
        }

        public async Task<bool> DeleteItemAsync(long id)
        {
            Item oldItem = await _repository.GetItemAsync(id);
            if (oldItem == null)
            {
                return false;
            }
            await _repository.ClearImages(oldItem);
            return await _repository.DeleteItemAsync(oldItem);
        }

        public async Task<ItemDTO> GetItemAsync(long id)
        {
            Item item = await _repository.GetItemAsync(id);

            if (item != null)
            {
                item.accessCount++;
                await _repository.UpdateItemAsync(item);
                return ItemToDTO(item);
            }
            return null;
        }

        public async Task<IEnumerable<ItemDTO>> GetItemsAsync()
        {
            IEnumerable<Item> items = await _repository.GetItemsAsync();
            List<ItemDTO> result = new List<ItemDTO>();
            foreach (Item i in items)
            {
                result.Add(ItemToDTO(i));
            }
            return result;
        }

        public async Task<ItemDTO> UpdateItemAsync(ItemDTO item)
        {
            Item oldItem = await _repository.GetItemAsync(item.Id);
            if (oldItem == null)
            {
                return null;
            }
            oldItem.Name = item.Name;
            oldItem.Description = item.Description;
            if (oldItem.Images != null && item.Images != null)
            {
                await _repository.ClearImages(oldItem);
            }
            if (item.Images != null)
            {
                oldItem.Images = new List<Image>();
                foreach (ImageDTO i in item.Images)
                {
                    Image image = DTOToImage(i);
                    image.Target = oldItem;
                    oldItem.Images.Add(image);
                }
            }
            oldItem.accessCount++;
            Item updatedItem = await _repository.UpdateItemAsync(oldItem);
            if (updatedItem == null)
            {
                return null;
            }
            return ItemToDTO(updatedItem);
        }

        public async Task<IEnumerable<ItemDTO>> QueryItemsAsync(String query)
        {
            IEnumerable<Item> items = await _repository.QueryItems(query);
            List<ItemDTO> itemDTOs = new List<ItemDTO>();
            foreach (Item i in items)
            {
                itemDTOs.Add(ItemToDTO(i));
            }
            return itemDTOs;
        }

        public async Task<IEnumerable<ItemDTO>> GetItemsAsync(string username)
        {
            User owner = await _userRepository.GetUserAsync(username);
            if(owner == null)
            {
                return null;
            }
            IEnumerable<Item> items = await _repository.GetItemsAsync(owner);
            List<ItemDTO> itemDTOs = new List<ItemDTO>();
            foreach(Item i in items)
            {
                itemDTOs.Add(ItemToDTO(i));
            }
            return itemDTOs;
        }

        private async Task<Item> DTOToItem(ItemDTO dto)
        {
            Item newItem = new Item();
            newItem.Name = dto.Name;
            newItem.Description = dto.Description;

            //Hae omistaja kannasta
            User owner = await _userRepository.GetUserAsync(dto.Owner);

            if (owner != null)
            {
                newItem.Owner = owner;
            }
            if(dto.Images != null)
            {
                newItem.Images = new List<Image>();
                foreach(ImageDTO i in dto.Images)
                {
                    newItem.Images.Add(DTOToImage(i));
                }
            }

            newItem.accessCount = 0;
            return newItem;
        }

        private ItemDTO ItemToDTO(Item item)
        {
            ItemDTO dto = new ItemDTO();
            dto.Id = item.Id;
            dto.Name = item.Name;
            dto.Description = item.Description;
            if(item.Images != null)
            {
                dto.Images = new List<ImageDTO>();
                foreach(Image i in item.Images)
                {
                    dto.Images.Add(ImageToDTO(i));
                }
            }
            if (item.Owner != null)
            {
                dto.Owner = item.Owner.UserName;
            }
            return dto;
        }

        private Image DTOToImage(ImageDTO dto)
        {
            Image image = new Image();
            image.Url = dto.Url;
            image.Description = dto.Description;
            return image;
        }

        private ImageDTO ImageToDTO(Image image)
        {
            ImageDTO dto = new ImageDTO();
            dto.Url = image.Url;
            dto.Description = image.Description;
            return dto;
        }

    }
}
