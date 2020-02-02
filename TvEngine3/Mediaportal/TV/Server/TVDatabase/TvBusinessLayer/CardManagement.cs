using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class CardManagement
  {
    public static IList<Card> ListAllCards()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations().ToList();
      }
    }

    /// <summary>
    /// Checks if a card can view a specific channel
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <param name="card"></param>
    /// <returns>true/false</returns>
    public static bool CanViewTvChannel(Card card, int channelId)
    {
      var cardChannels = card.ChannelMaps;
      return cardChannels.Any(cmap => channelId == cmap.ChannelId && !cmap.EpgOnly);
    }

    /// <summary>
    /// Checks if a card can tune a specific channel
    /// </summary>
    /// <param name="card"></param>
    /// <param name="channelId">Channel id</param>
    /// <returns>true/false</returns>
    public static bool CanTuneTvChannel(Card card, int channelId)
    {
      var cardChannels = card.ChannelMaps;
      return cardChannels.Any(cmap => channelId == cmap.ChannelId);
    }

    public static Card GetCardByDevicePath(string devicePath)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations().FirstOrDefault(c => c.DevicePath == devicePath);
      }
    }


    public static Card GetCardByDevicePath(string devicePath, CardIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations(includeRelations).FirstOrDefault(c => c.DevicePath == devicePath);
      }
    }

    public static Card SaveCard(Card card)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(card);
        context.SaveChanges(true);
        return card;
      }
    }

    public static void DeleteCard(int idCard)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var card = context.Cards.FirstOrDefault(p => p.CardId == idCard);
        if (card != null)
        {
          context.Cards.Remove(card);
          context.SaveChanges(true);
        }
      }
    }

    public static IList<CardGroup> ListAllCardGroups()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.CardGroups.ToList();
      }
    }

    public static DisEqcMotor SaveDisEqcMotor(DisEqcMotor motor)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(motor);
        context.SaveChanges(true);
        return motor;
      }
    }

    public static Card GetCard(int idCard)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations().FirstOrDefault(c => c.CardId == idCard);
      }
    }

    public static Card GetCard(int idCard, CardIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations(includeRelations).FirstOrDefault(c => c.CardId == idCard);
      }
    }

    public static CardGroup SaveCardGroup(CardGroup @group)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(@group);
        context.SaveChanges(true);
        return @group;
      }
    }

    public static void DeleteCardGroup(int idCardGroup)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var cardGroup = context.CardGroups.FirstOrDefault(p => p.CardGroupId == idCardGroup);
        if (cardGroup != null)
        {
          context.CardGroups.Remove(cardGroup);
          context.SaveChanges(true);
        }
      }
    }

    public static IList<SoftwareEncoder> ListAllSoftwareEncodersVideo()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.SoftwareEncoders.Where(s => s.Type == 0).OrderBy(s => s.Priority).ToList();
      }
    }

    public static IList<SoftwareEncoder> ListAllSoftwareEncodersAudio()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.SoftwareEncoders.Where(s => s.Type == 1).OrderBy(s => s.Priority).ToList();
      }
    }

    public static IList<Satellite> ListAllSatellites()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Satellites.ToList();
      }
    }

    public static Satellite SaveSatellite(Satellite satellite)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(satellite);
        context.SaveChanges(true);
        return satellite;
      }
    }

    public static SoftwareEncoder SaveSoftwareEncoder(SoftwareEncoder encoder)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(encoder);
        context.SaveChanges(true);
        return encoder;
      }
    }

    public static void DeleteGroupMap(int idMap)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var map = context.CardGroupMaps.FirstOrDefault(p => p.CardGroupMapId == idMap);
        if (map != null)
        {
          context.CardGroupMaps.Remove(map);
          context.SaveChanges(true);
        }
      }
    }

    public static CardGroupMap SaveCardGroupMap(CardGroupMap map)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(map);
        context.SaveChanges(true);
        return map;
      }
    }

    public static IList<Card> ListAllCards(CardIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Cards.IncludeAllRelations(includeRelations).ToList();
      }
    }

    public static IList<Card> SaveCards(IEnumerable<Card> cards)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.UpdateRange(cards);
        context.SaveChanges(true);
        return cards.ToList();
      }
    }
  }
}
